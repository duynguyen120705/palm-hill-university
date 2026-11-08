using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class ImportService : IImportService
    {
        // Cột của file mẫu (FR-30), đúng thứ tự
        private static readonly string[] CotMau =
        {
            "MaSV", "HoTen", "NgaySinh", "GioiTinh", "MaLop", "SDT", "Email", "DiaChi", "TrangThai", "MatKhau"
        };

        private const int SoDongToiDa = 2000;

        // Giới hạn thời gian chạy Regex trên dữ liệu từ file nhập (chống ReDoS)
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        private const int DangHoc = MaTrangThai.DangHoc, BaoLuu = MaTrangThai.BaoLuu,
            ThoiHoc = MaTrangThai.ThoiHoc, DaTotNghiep = MaTrangThai.DaTotNghiep;

        private readonly AppDbContext _context;
        private readonly IAccountService _account;
        private readonly IScopeService _scope;
        private readonly IAuditLogService _auditLog;

        public ImportService(AppDbContext context, IAccountService account, IScopeService scope, IAuditLogService auditLog)
        {
            _context = context;
            _account = account;
            _scope = scope;
            _auditLog = auditLog;
        }

        public byte[] CreateTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("SinhVien");
            for (int i = 0; i < CotMau.Length; i++)
                ws.Cell(1, i + 1).Value = CotMau[i];

            ws.Cell(2, 1).Value = "26019999";
            ws.Cell(2, 2).Value = "Nguyễn Văn A";
            ws.Cell(2, 3).Value = "15/08/2008";
            ws.Cell(2, 4).Value = "Nam";
            ws.Cell(2, 5).Value = "CNTT-K26A";
            ws.Cell(2, 6).Value = "0912345678";
            ws.Cell(2, 7).Value = "nguyenvana@example.com";
            ws.Cell(2, 8).Value = "Hà Nội";
            ws.Cell(2, 9).Value = "Đang học";
            ws.Cell(2, 10).Value = "Student@123";

            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<KetQuaNhapDto> ImportAsync(Stream file, string tenFile)
        {
            var scope = _scope.GetCurrent();
            var ketQua = new KetQuaNhapDto { TenFile = tenFile };

            // File chỉ đọc trong bộ nhớ, không lưu xuống đĩa (NFR-08)
            using var wb = new XLWorkbook(file);
            var ws = wb.Worksheets.First();

            for (int i = 0; i < CotMau.Length; i++)
            {
                if (!string.Equals(ws.Cell(1, i + 1).GetString().Trim(), CotMau[i], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"File sai mẫu: cột {i + 1} phải là '{CotMau[i]}'.");
            }

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow < 2) throw new InvalidDataException("File không có dòng dữ liệu nào.");
            if (lastRow - 1 > SoDongToiDa) throw new InvalidDataException($"File vượt quá {SoDongToiDa} dòng dữ liệu.");

            // Mã SV lặp trong file: báo lỗi tất cả dòng mang mã đó
            var maTrongFile = Enumerable.Range(2, lastRow - 1)
                .Select(r => ws.Cell(r, 1).GetString().Trim().ToUpperInvariant())
                .Where(m => m.Length > 0)
                .GroupBy(m => m)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            // Lớp trong phạm vi được phép (FR-09); global filter đã loại lớp xóa mềm
            var lopDuocPhep = (await _scope.ApplyClassScope(_context.Lops.AsNoTracking())
                .Select(l => l.MaLop).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int r = 2; r <= lastRow; r++)
            {
                if (ws.Row(r).IsEmpty()) continue;

                var maSV = ws.Cell(r, 1).GetString().Trim().ToUpperInvariant();
                try
                {
                    if (maTrongFile.Contains(maSV))
                        throw new DongLoiException("Mã SV bị lặp trong file.");

                    var dong = DocDong(ws, r);
                    var themMoi = await XuLyDongAsync(dong, lopDuocPhep, scope.IsAdmin);
                    if (themMoi) ketQua.SoThemMoi++; else ketQua.SoCapNhat++;
                }
                catch (DongLoiException ex)
                {
                    ketQua.Loi.Add(new DongLoiDto { Dong = r, MaSV = maSV, Loi = ex.Message });
                }
                finally
                {
                    _context.ChangeTracker.Clear();
                }
            }

            ketQua.SoLoi = ketQua.Loi.Count;

            var phien = new PhienNhap
            {
                ThoiDiem = DateTime.UtcNow,
                Username = scope.UserName,
                TenFile = tenFile,
                SoThemMoi = ketQua.SoThemMoi,
                SoCapNhat = ketQua.SoCapNhat,
                SoLoi = ketQua.SoLoi,
                ChiTietLoi = ketQua.Loi.Count > 0 ? JsonSerializer.Serialize(ketQua.Loi) : null
            };
            _context.PhienNhaps.Add(phien);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Import", "PhienNhap", phien.Id.ToString());
            await _context.SaveChangesAsync();

            ketQua.PhienNhapId = phien.Id;
            return ketQua;
        }

        public async Task<PagedResultDto<PhienNhapItemDto>> ListSessionsAsync(int page, int pageSize)
        {
            var scope = _scope.GetCurrent();
            var query = _context.PhienNhaps.AsNoTracking();
            if (!scope.IsAdmin)
                query = query.Where(p => p.Username == scope.UserName);

            return await query
                .OrderByDescending(p => p.ThoiDiem).ThenByDescending(p => p.Id)
                .ToPagedResultAsync(page, pageSize, p => new PhienNhapItemDto
                {
                    Id = p.Id,
                    ThoiDiemFormatted = DateTime.SpecifyKind(p.ThoiDiem, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Username = p.Username,
                    TenFile = p.TenFile,
                    SoThemMoi = p.SoThemMoi,
                    SoCapNhat = p.SoCapNhat,
                    SoLoi = p.SoLoi
                });
        }

        // Xóa bản ghi lịch sử nhập; không ảnh hưởng hồ sơ sinh viên đã nhập
        public async Task DeleteSessionAsync(long id)
        {
            var phien = await _context.PhienNhaps.FindAsync(id)
                ?? throw new NghiepVuException("Không tìm thấy phiên nhập.", 404);

            var scope = _scope.GetCurrent();
            if (!scope.IsAdmin && phien.Username != scope.UserName)
                throw new NghiepVuException("Bạn chỉ được xóa phiên nhập của mình.", 403);

            _context.PhienNhaps.Remove(phien);
            await _auditLog.LogAsync("XoaPhienNhap", "PhienNhap", id.ToString());
            await _context.SaveChangesAsync();
        }

        // ===================== Xử lý từng dòng =====================

        private sealed class DongLoiException : Exception
        {
            public DongLoiException(string message) : base(message) { }
        }

        private sealed record DongNhap(string MaSV, string HoTen, DateTime NgaySinh, int GioiTinh, string MaLop,
            string? SDT, string? Email, string? DiaChi, int TrangThai, string? MatKhau);

        private static DongNhap DocDong(IXLWorksheet ws, int r)
        {
            string? Text(int c)
            {
                var s = ws.Cell(r, c).GetString().Trim();
                return s.Length == 0 ? null : s;
            }

            var loi = new List<string>();

            var maSV = (Text(1) ?? "").ToUpperInvariant();
            if (!Regex.IsMatch(maSV, "^[A-Z0-9]{6,15}$", RegexOptions.None, RegexTimeout))
                loi.Add("Mã SV phải 6–15 ký tự chữ và số không dấu");

            var hoTen = Text(2) ?? "";
            if (hoTen.Length < 2 || hoTen.Length > 100 || hoTen.Any(char.IsDigit))
                loi.Add("Họ tên bắt buộc, 2–100 ký tự, không chứa chữ số");

            var ngaySinh = DocNgay(ws.Cell(r, 3));
            if (ngaySinh == null)
                loi.Add("Ngày sinh bắt buộc, định dạng dd/MM/yyyy");
            else if (ngaySinh > DateTime.Today.AddYears(-15))
                loi.Add("Ngày sinh không hợp lệ (tuổi tối thiểu 15)");

            var gioiTinh = TextHelper.BoDau(Text(4)) switch
            {
                "nu" => MaTrangThai.Nu,
                "nam" => MaTrangThai.Nam,
                "khac" => MaTrangThai.Khac,
                _ => -1
            };
            if (gioiTinh < 0) loi.Add("Giới tính phải là Nam/Nữ/Khác");

            var maLop = Text(5) ?? "";
            if (maLop.Length == 0) loi.Add("Mã lớp bắt buộc");

            var sdt = Text(6);
            if (sdt != null && !Regex.IsMatch(sdt, @"^0\d{9}$", RegexOptions.None, RegexTimeout))
                loi.Add("SĐT gồm 10 chữ số, bắt đầu bằng 0");

            var email = Text(7)?.ToLowerInvariant();
            if (email != null && (email.Length > 100 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, RegexTimeout)))
                loi.Add("Email không đúng định dạng");

            var trangThai = TextHelper.BoDau(Text(9)) switch
            {
                "0" or "dang hoc" => DangHoc,
                "1" or "bao luu" => BaoLuu,
                "2" or "thoi hoc" => ThoiHoc,
                "3" or "da tot nghiep" => DaTotNghiep,
                _ => -1
            };
            if (trangThai < 0) loi.Add("Trạng thái phải là Đang học/Bảo lưu/Thôi học/Đã tốt nghiệp");

            if (loi.Count > 0) throw new DongLoiException(string.Join("; ", loi) + ".");

            return new DongNhap(maSV, hoTen, ngaySinh!.Value, gioiTinh, maLop, sdt, email, Text(8), trangThai, Text(10));
        }

        // Trả về true nếu thêm mới, false nếu cập nhật
        private async Task<bool> XuLyDongAsync(DongNhap d, HashSet<string> lopDuocPhep, bool isAdmin)
        {
            var hienCo = await _context.SinhViens.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.MaSV == d.MaSV);

            if (hienCo != null && hienCo.DaXoa)
                throw new DongLoiException("Mã SV trùng với hồ sơ đã xóa mềm, không được dùng lại.");
            if (!lopDuocPhep.Contains(d.MaLop))
                throw new DongLoiException($"Lớp '{d.MaLop}' không tồn tại hoặc ngoài phạm vi được phép.");
            if (hienCo != null && !lopDuocPhep.Contains(hienCo.MaLop))
                throw new DongLoiException("Hồ sơ hiện có nằm ngoài phạm vi được phép.");

            if (d.Email != null && await _context.SinhViens.IgnoreQueryFilters()
                    .AnyAsync(s => s.Email == d.Email && s.MaSV != d.MaSV))
                throw new DongLoiException("Email đã được sinh viên khác sử dụng.");

            return hienCo == null
                ? await ThemMoiAsync(d)
                : await CapNhatAsync(hienCo, d, isAdmin);
        }

        // Thêm hồ sơ và tài khoản trong cùng một giao dịch (theo FR-13)
        private async Task<bool> ThemMoiAsync(DongNhap d)
        {
            if (d.MatKhau == null)
                throw new DongLoiException("Dòng thêm mới bắt buộc có mật khẩu ban đầu.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SinhViens.Add(new SinhVien
                {
                    MaSV = d.MaSV,
                    HoTen = d.HoTen,
                    NgaySinh = d.NgaySinh,
                    GioiTinh = d.GioiTinh,
                    MaLop = d.MaLop,
                    SDT = d.SDT,
                    Email = d.Email,
                    DiaChi = d.DiaChi,
                    TrangThai = d.TrangThai,
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow
                });
                await _auditLog.LogAsync("ImportTaoHoSo", "SinhVien", d.MaSV);
                await _context.SaveChangesAsync();

                // Lỗi tạo tài khoản: rollback cả hồ sơ mới (FR-13)
                var taoTaiKhoan = await _account.CreateStudentAccountAsync(d.MaSV, d.MatKhau);
                if (!taoTaiKhoan.Succeeded)
                    throw new DongLoiException("Tạo tài khoản thất bại: " + taoTaiKhoan.Error);

                // Hồ sơ Thôi học: tài khoản ở trạng thái khóa (FR-16)
                if (d.TrangThai == ThoiHoc)
                {
                    var khoa = await _account.SetLockAsync(d.MaSV, true, "Nhập Excel: hồ sơ Thôi học");
                    if (!khoa.Succeeded)
                        throw new DongLoiException("Không khóa được tài khoản sinh viên Thôi học.");
                }

                await tx.CommitAsync();
                return true;
            }
            catch (DongLoiException)
            {
                await tx.RollbackAsync();
                throw;
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                throw new DongLoiException("Không lưu được hồ sơ (vi phạm ràng buộc dữ liệu).");
            }
        }

        // Cập nhật hồ sơ theo Mã SV; giữ nguyên tài khoản và mật khẩu.
        // Đổi lớp theo FR-17 (lớp hiện tại và lớp đích đều đã được kiểm tra phạm vi ở XuLyDongAsync),
        // đổi trạng thái theo FR-16; không tự mở khóa tài khoản (FR-20)
        private async Task<bool> CapNhatAsync(SinhVien sv, DongNhap d, bool isAdmin)
        {
            var doiLop = !string.Equals(sv.MaLop, d.MaLop, StringComparison.OrdinalIgnoreCase);
            var doiTrangThai = sv.TrangThai != d.TrangThai;
            if (doiTrangThai)
            {
                var loi = KiemTraChuyenTrangThai(sv.TrangThai, d.TrangThai, isAdmin);
                if (loi != null) throw new DongLoiException(loi);
            }

            if (doiLop) sv.MaLop = d.MaLop;
            if (doiTrangThai) sv.TrangThai = d.TrangThai;
            sv.HoTen = d.HoTen;
            sv.NgaySinh = d.NgaySinh;
            sv.GioiTinh = d.GioiTinh;
            // Ô trống ở trường liên hệ tùy chọn giữ giá trị hiện có
            if (d.SDT != null) sv.SDT = d.SDT;
            if (d.Email != null) sv.Email = d.Email;
            if (d.DiaChi != null) sv.DiaChi = d.DiaChi;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _auditLog.LogAsync("ImportCapNhatHoSo", "SinhVien", sv.MaSV);
                if (doiLop) await _auditLog.LogAsync("ChuyenLop", "SinhVien", sv.MaSV);
                if (doiTrangThai) await _auditLog.LogAsync("DoiTrangThai", "SinhVien", sv.MaSV);
                await _context.SaveChangesAsync();

                // Chuyển sang Thôi học tự động khóa tài khoản (FR-16)
                if (doiTrangThai && d.TrangThai == ThoiHoc)
                {
                    var userName = await _context.Users.AsNoTracking()
                        .Where(u => u.MaSV == sv.MaSV && u.TrangThaiTaiKhoan != MaTrangThai.TkBiKhoa)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync();
                    if (userName != null)
                    {
                        var khoa = await _account.SetLockAsync(userName, true, "Nhập Excel: chuyển sang Thôi học");
                        if (!khoa.Succeeded)
                            throw new DongLoiException("Không khóa được tài khoản sinh viên Thôi học.");
                    }
                }

                await tx.CommitAsync();
                return false;
            }
            catch (DongLoiException)
            {
                await tx.RollbackAsync();
                throw;
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                throw new DongLoiException("Không lưu được hồ sơ (vi phạm ràng buộc dữ liệu).");
            }
        }

        // Quy tắc chuyển trạng thái FR-16; trả về thông báo lỗi hoặc null nếu hợp lệ
        private static string? KiemTraChuyenTrangThai(int cu, int moi, bool isAdmin)
        {
            switch (cu)
            {
                case DangHoc when moi is BaoLuu or DaTotNghiep or ThoiHoc:
                case BaoLuu when moi is DangHoc or ThoiHoc:
                    return null;
                case ThoiHoc when moi == DangHoc:
                    return isAdmin ? null : "Chỉ Admin được chuyển Thôi học → Đang học.";
                case DaTotNghiep:
                    // Admin sửa trạng thái hồ sơ Đã tốt nghiệp bắt buộc kèm lý do - file import không có cột lý do
                    return "Hồ sơ Đã tốt nghiệp chỉ được Admin sửa trạng thái kèm lý do qua chức năng Đổi trạng thái.";
                default:
                    return "Chuyển trạng thái không hợp lệ theo quy tắc FR-16.";
            }
        }

        private static DateTime? DocNgay(IXLCell cell)
        {
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime().Date;
            var s = cell.GetString().Trim();
            return DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        }
    }
}
