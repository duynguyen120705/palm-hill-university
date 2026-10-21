using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    public class XuatNhapService : IXuatNhapService
    {
        // Cột của file mẫu (FR-30), đúng thứ tự
        private static readonly string[] CotMau =
        {
            "MaSV", "HoTen", "NgaySinh", "GioiTinh", "MaLop", "SDT", "Email", "DiaChi", "TrangThai", "MatKhau"
        };

        private const int SoDongToiDa = 2000;

        // Quy ước mã theo dữ liệu seed: GioiTinh 0=Nữ 1=Nam 2=Khác
        private const int DangHoc = 0, BaoLuu = 1, ThoiHoc = 2, DaTotNghiep = 3;

        private readonly ApplicationDbContext _context;
        private readonly ITaiKhoanService _taiKhoan;
        private readonly IPhamViDuLieuService _phamVi;
        private readonly INhatKyService _nhatKy;

        public XuatNhapService(ApplicationDbContext context, ITaiKhoanService taiKhoan,
            IPhamViDuLieuService phamVi, INhatKyService nhatKy)
        {
            _context = context;
            _taiKhoan = taiKhoan;
            _phamVi = phamVi;
            _nhatKy = nhatKy;
        }

        public byte[] TaoFileMau()
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

        public async Task<KetQuaNhapDto> NhapExcelAsync(Stream file, string tenFile, ClaimsPrincipal user)
        {
            var username = user.Identity?.Name ?? "unknown";
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
            var lopDuocPhep = (await (await _phamVi.LocLopAsync(_context.Lops.AsNoTracking(), user))
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
                    var themMoi = await XuLyDongAsync(dong, lopDuocPhep, username, user.IsInRole("Admin"));
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
                Username = username,
                TenFile = tenFile,
                SoThemMoi = ketQua.SoThemMoi,
                SoCapNhat = ketQua.SoCapNhat,
                SoLoi = ketQua.SoLoi,
                ChiTietLoi = ketQua.Loi.Count > 0 ? JsonSerializer.Serialize(ketQua.Loi) : null
            };
            _context.PhienNhaps.Add(phien);
            await _context.SaveChangesAsync();
            await _nhatKy.GhiAsync(username, "Import", "PhienNhap", phien.Id.ToString());

            ketQua.PhienNhapId = phien.Id;
            return ketQua;
        }

        public async Task<PagedResultDto<PhienNhapItemDto>> DanhSachPhienNhapAsync(int page, int pageSize, ClaimsPrincipal user)
        {
            var query = _context.PhienNhaps.AsNoTracking();
            if (!user.IsInRole("Admin"))
            {
                var username = user.Identity?.Name;
                query = query.Where(p => p.Username == username);
            }

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

        public async Task<PagedResultDto<SinhVienItemDto>> DanhSachSinhVienAsync(DanhSachFilterDto filter, ClaimsPrincipal user)
        {
            var query = await LocDanhSachAsync(filter, user);

            return await query
                .OrderBy(s => s.MaSV)
                .ToPagedResultAsync(filter.Page, filter.PageSize, s => new SinhVienItemDto
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    GioiTinh = s.GioiTinh,
                    GioiTinhText = s.GioiTinh == 1 ? "Nam" : (s.GioiTinh == 0 ? "Nữ" : "Khác"),
                    MaLop = s.MaLop,
                    TenLop = s.Lop!.TenLop,
                    SDT = s.SDT,
                    Email = s.Email,
                    TrangThai = s.TrangThai,
                    TrangThaiText = s.TrangThai == 0 ? "Đang học" :
                                   (s.TrangThai == 1 ? "Bảo lưu" :
                                   (s.TrangThai == 2 ? "Thôi học" : "Đã tốt nghiệp"))
                });
        }

        // FR-28: truy vấn lại toàn bộ kết quả lọc (không phân trang) trong phạm vi được phép
        public async Task<FileXuat?> XuatDanhSachAsync(DanhSachFilterDto filter, bool pdf, ClaimsPrincipal user)
        {
            var query = await LocDanhSachAsync(filter, user);
            var ds = await query
                .OrderBy(s => s.MaSV)
                .Select(s => new { s.MaSV, s.HoTen, s.NgaySinh, s.GioiTinh, s.MaLop, s.Lop!.TenLop, s.SDT, s.Email, s.DiaChi, s.TrangThai })
                .ToListAsync();
            if (ds.Count == 0) return null;

            string[] cot = { "STT", "Mã SV", "Họ tên", "Ngày sinh", "Giới tính", "Lớp", "SĐT", "Email", "Địa chỉ", "Trạng thái" };
            var dong = ds.Select((s, i) => new object?[]
            {
                i + 1, s.MaSV, s.HoTen, s.NgaySinh.ToString("dd/MM/yyyy"),
                s.GioiTinh == 1 ? "Nam" : (s.GioiTinh == 0 ? "Nữ" : "Khác"),
                s.TenLop + " (" + s.MaLop + ")", s.SDT, s.Email, s.DiaChi, TenTrangThai(s.TrangThai)
            }).ToList();

            var phamVi = MoTaPhamVi(filter, user);
            var tieuDe = "DANH SÁCH SINH VIÊN";
            var moTa = $"{phamVi} | Tổng: {ds.Count} sinh viên | Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm} bởi {user.Identity?.Name}";
            var tenFile = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmm}";

            var file = pdf
                ? new FileXuat(BaoCaoFileBuilder.TaoPdf(tieuDe, moTa, cot, dong), BaoCaoFileBuilder.PdfContentType, tenFile + ".pdf")
                : new FileXuat(BaoCaoFileBuilder.TaoExcel(tieuDe, moTa, cot, dong), BaoCaoFileBuilder.ExcelContentType, tenFile + ".xlsx");

            // Ghi nhật ký lần xuất: người xuất, phạm vi, thời điểm (không ghi từ khóa vì có thể là dữ liệu cá nhân)
            await _nhatKy.GhiAsync(user.Identity?.Name ?? "unknown", pdf ? "XuatDanhSachPdf" : "XuatDanhSachExcel",
                "DanhSach", CatChuoi(phamVi, 100));
            return file;
        }

        // Mô tả phạm vi xuất dùng cho tiêu đề file và nhật ký
        internal static string MoTaPhamVi(ThongKeFilterDto filter, ClaimsPrincipal user)
        {
            var phan = new List<string> { user.IsInRole("Admin") ? "Toàn trường" : "Khoa phụ trách" };
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa)) phan.Add("Khoa " + filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc)) phan.Add("Khóa " + filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop)) phan.Add("Lớp " + filter.MaLop);
            if (filter.TrangThai.HasValue) phan.Add(TenTrangThai(filter.TrangThai.Value));
            if (filter is DanhSachFilterDto ds && !string.IsNullOrWhiteSpace(ds.Keyword)) phan.Add("có từ khóa");
            return string.Join(", ", phan);
        }

        internal static string TenTrangThai(int trangThai) => trangThai switch
        {
            DangHoc => "Đang học",
            BaoLuu => "Bảo lưu",
            ThoiHoc => "Thôi học",
            DaTotNghiep => "Đã tốt nghiệp",
            _ => "Không xác định"
        };

        internal static string CatChuoi(string s, int max) => s.Length <= max ? s : s[..max];

        // Áp phạm vi (FR-09) rồi tiêu chí lọc; dùng chung cho xem trước và xuất toàn bộ kết quả ra file
        private async Task<IQueryable<SinhVien>> LocDanhSachAsync(DanhSachFilterDto filter, ClaimsPrincipal user)
        {
            var query = await _phamVi.LocSinhVienAsync(_context.SinhViens.AsNoTracking(), user);

            // Không phân biệt hoa thường và dấu ("nguyen" khớp "Nguyễn", "do" khớp "Đỗ")
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(s =>
                    EF.Functions.Collate(s.HoTen, TimKiemKhongDau.Collation).Contains(kw) ||
                    s.MaSV.Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
                query = query.Where(s => s.Lop!.MaKhoa == filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc))
                query = query.Where(s => s.Lop!.MaKhoaHoc == filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop))
                query = query.Where(s => s.MaLop == filter.MaLop);
            if (filter.TrangThai.HasValue)
                query = query.Where(s => s.TrangThai == filter.TrangThai.Value);

            return query;
        }

        // Xóa bản ghi lịch sử nhập; không ảnh hưởng hồ sơ sinh viên đã nhập
        public async Task<(bool Ok, int StatusCode, string Message)> XoaPhienNhapAsync(long id, ClaimsPrincipal user)
        {
            var phien = await _context.PhienNhaps.FindAsync(id);
            if (phien == null) return (false, 404, "Không tìm thấy phiên nhập.");

            var username = user.Identity?.Name ?? "unknown";
            if (!user.IsInRole("Admin") && phien.Username != username)
                return (false, 403, "Bạn chỉ được xóa phiên nhập của mình.");

            _context.PhienNhaps.Remove(phien);
            await _context.SaveChangesAsync();
            await _nhatKy.GhiAsync(username, "XoaPhienNhap", "PhienNhap", id.ToString());

            return (true, 200, $"Đã xóa phiên nhập #{id}.");
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
            if (!Regex.IsMatch(maSV, "^[A-Z0-9]{6,15}$"))
                loi.Add("Mã SV phải 6–15 ký tự chữ và số không dấu");

            var hoTen = Text(2) ?? "";
            if (hoTen.Length < 2 || hoTen.Length > 100 || hoTen.Any(char.IsDigit))
                loi.Add("Họ tên bắt buộc, 2–100 ký tự, không chứa chữ số");

            var ngaySinh = DocNgay(ws.Cell(r, 3));
            if (ngaySinh == null)
                loi.Add("Ngày sinh bắt buộc, định dạng dd/MM/yyyy");
            else if (ngaySinh > DateTime.Today.AddYears(-15))
                loi.Add("Ngày sinh không hợp lệ (tuổi tối thiểu 15)");

            var gioiTinh = BoDau(Text(4)) switch { "nu" => 0, "nam" => 1, "khac" => 2, _ => -1 };
            if (gioiTinh < 0) loi.Add("Giới tính phải là Nam/Nữ/Khác");

            var maLop = Text(5) ?? "";
            if (maLop.Length == 0) loi.Add("Mã lớp bắt buộc");

            var sdt = Text(6);
            if (sdt != null && !Regex.IsMatch(sdt, @"^0\d{9}$"))
                loi.Add("SĐT gồm 10 chữ số, bắt đầu bằng 0");

            var email = Text(7)?.ToLowerInvariant();
            if (email != null && (email.Length > 100 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")))
                loi.Add("Email không đúng định dạng");

            var trangThai = BoDau(Text(9)) switch
            {
                "0" or "dang hoc" => 0,
                "1" or "bao luu" => 1,
                "2" or "thoi hoc" => 2,
                "3" or "da tot nghiep" => 3,
                _ => -1
            };
            if (trangThai < 0) loi.Add("Trạng thái phải là Đang học/Bảo lưu/Thôi học/Đã tốt nghiệp");

            if (loi.Count > 0) throw new DongLoiException(string.Join("; ", loi) + ".");

            return new DongNhap(maSV, hoTen, ngaySinh!.Value, gioiTinh, maLop, sdt, email, Text(8), trangThai, Text(10));
        }

        // Trả về true nếu thêm mới, false nếu cập nhật
        private async Task<bool> XuLyDongAsync(DongNhap d, HashSet<string> lopDuocPhep, string username, bool isAdmin)
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
                ? await ThemMoiAsync(d, username)
                : await CapNhatAsync(hienCo, d, username, isAdmin);
        }

        // Thêm hồ sơ và tài khoản trong cùng một giao dịch (theo FR-13)
        private async Task<bool> ThemMoiAsync(DongNhap d, string username)
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
                await _context.SaveChangesAsync();

                // Hồ sơ Thôi học: tài khoản tạo ra ở trạng thái khóa (FR-16)
                await _taiKhoan.TaoTaiKhoanSinhVienAsync(d.MaSV, d.Email, d.MatKhau, khoa: d.TrangThai == ThoiHoc);

                await _nhatKy.GhiAsync(username, "ImportTaoHoSo", "SinhVien", d.MaSV);
                await _nhatKy.GhiAsync(username, "TaoTaiKhoan", "TaiKhoan", d.MaSV);
                if (d.TrangThai == ThoiHoc)
                    await _nhatKy.GhiAsync(username, "KhoaTaiKhoan", "TaiKhoan", d.MaSV);
                await tx.CommitAsync();
                return true;
            }
            catch (DongLoiException)
            {
                await tx.RollbackAsync();
                throw;
            }
            catch (NghiepVuException ex)
            {
                // Lỗi tạo tài khoản: rollback cả hồ sơ mới (FR-13)
                await tx.RollbackAsync();
                throw new DongLoiException("Tạo tài khoản thất bại: " + ex.Message);
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
        private async Task<bool> CapNhatAsync(SinhVien sv, DongNhap d, string username, bool isAdmin)
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
                await _context.SaveChangesAsync();
                await _nhatKy.GhiAsync(username, "ImportCapNhatHoSo", "SinhVien", sv.MaSV);
                if (doiLop) await _nhatKy.GhiAsync(username, "ChuyenLop", "SinhVien", sv.MaSV);
                if (doiTrangThai) await _nhatKy.GhiAsync(username, "DoiTrangThai", "SinhVien", sv.MaSV);

                // Chuyển sang Thôi học tự động khóa tài khoản (FR-16)
                if (doiTrangThai && d.TrangThai == ThoiHoc)
                {
                    var taiKhoan = await _taiKhoan.TimTheoMaSVAsync(sv.MaSV);
                    if (taiKhoan != null && taiKhoan.TrangThaiTaiKhoan != ITaiKhoanService.BiKhoa)
                    {
                        await _taiKhoan.KhoaAsync(taiKhoan);
                        await _nhatKy.GhiAsync(username, "KhoaTaiKhoan", "TaiKhoan", taiKhoan.UserName ?? sv.MaSV);
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
            catch (NghiepVuException)
            {
                await tx.RollbackAsync();
                throw new DongLoiException("Không khóa được tài khoản sinh viên Thôi học.");
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

        // Bỏ dấu tiếng Việt, chữ thường - để so khớp Giới tính/Trạng thái
        private static string BoDau(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var chuan = s.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in chuan)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return Regex.Replace(sb.ToString(), @"\s+", " ");
        }
    }
}
