using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
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
        private const string CotMaSV = "MaSV";
        private const string CotNgaySinh = "NgaySinh";
        private const string CotGioiTinh = "GioiTinh";
        private const string CotMaLop = "MaLop";
        private const string CotTrangThai = "TrangThai";
        private const string CotMatKhau = "MatKhauBanDau";

        // Cột của file mẫu BM02, đúng tên và thứ tự A -> J
        private static readonly string[] CotMau =
        {
            CotMaSV, "HoTen", CotNgaySinh, CotGioiTinh, CotMaLop, "SDT", "Email", "DiaChi", CotTrangThai, CotMatKhau
        };

        private readonly AppDbContext _context;
        private readonly IStudentService _students;
        private readonly IScopeService _scope;
        private readonly IAuditLogService _auditLog;

        public ImportService(AppDbContext context, IStudentService students, IScopeService scope, IAuditLogService auditLog)
        {
            _context = context;
            _students = students;
            _scope = scope;
            _auditLog = auditLog;
        }

        public byte[] CreateTemplate()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("DanhSach");
            // Định dạng văn bản để Excel giữ số 0 đầu của mã SV, SĐT và không đổi kiểu ngày
            ws.Columns(1, CotMau.Length).Style.NumberFormat.Format = "@";

            for (int i = 0; i < CotMau.Length; i++)
                ws.Cell(1, i + 1).Value = CotMau[i];

            string[][] dongMau =
            {
                new[] { "2310020001", "Nguyễn Văn An", "15/03/2005", "Nam", "2310A02", "0912345678", "an.nv@example.com", "Hà Nội", "Đang học", "Sv@12345" },
                new[] { "2310020002", "Trần Thị Bình", "02/11/2005", "Nữ", "2310A02", "", "", "", "Đang học", "Sv@12345" }
            };
            for (int r = 0; r < dongMau.Length; r++)
                for (int c = 0; c < CotMau.Length; c++)
                    ws.Cell(r + 2, c + 1).Value = dongMau[r][c];

            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<ImportResultDto> ImportAsync(Stream file, string tenFile)
        {
            // File chỉ đọc trong bộ nhớ, không lưu xuống đĩa (NFR-08); BM02: chỉ đọc sheet đầu tiên
            using var wb = MoFile(file);
            var ws = wb.Worksheets.First();
            KiemTraTieuDe(ws);

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var cacDong = Enumerable.Range(2, Math.Max(0, lastRow - 1))
                .Where(r => !ws.Row(r).IsEmpty())
                .ToList();

            // Mã SV lặp trong file: báo lỗi tất cả dòng mang mã đó
            var dongLap = cacDong
                .GroupBy(r => DocMaSV(ws, r))
                .Where(g => g.Key.Length > 0 && g.Count() > 1)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Global filter đã loại lớp xóa mềm; so khớp không phân biệt hoa thường rồi dùng mã chuẩn trong CSDL
            var lopDangDung = (await _context.Lops.AsNoTracking().Select(l => l.MaLop).ToListAsync())
                .ToDictionary(m => m, m => m, StringComparer.OrdinalIgnoreCase);
            var lopDuocPhep = (await _scope.ApplyClassScope(_context.Lops.AsNoTracking()).Select(l => l.MaLop).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var scope = _scope.GetCurrent();
            var thoiDiem = DateTime.Now;
            var ketQua = new ImportResultDto
            {
                TenFile = tenFile,
                NguoiNhap = scope.UserName,
                ThoiDiem = thoiDiem.ToString("dd/MM/yyyy HH:mm"),
                TongSoDong = cacDong.Count
            };

            foreach (var r in cacDong)
            {
                var maSV = DocMaSV(ws, r);
                try
                {
                    if (dongLap.TryGetValue(maSV, out var cungMa))
                    {
                        var dongKhac = string.Join(", ", cungMa.Where(d => d != r));
                        throw new DongLoiException(CotMaSV, $"Mã SV lặp trong file (trùng dòng {dongKhac})");
                    }

                    if (await XuLyDongAsync(ws, r, maSV, lopDangDung, lopDuocPhep)) ketQua.SoThemMoi++;
                    else ketQua.SoCapNhat++;
                }
                catch (DongLoiException ex)
                {
                    GhiLoi(ketQua, r, maSV, ex.Loi);
                }
                catch (NghiepVuException ex)
                {
                    GhiLoi(ketQua, r, maSV, new[] { new LoiCot(ex.Truong ?? CotMaSV, ex.Message) });
                }
                catch (DbUpdateException)
                {
                    GhiLoi(ketQua, r, maSV, new[] { new LoiCot(CotMaSV, "Không lưu được hồ sơ (vi phạm ràng buộc dữ liệu)") });
                }
                finally
                {
                    // Dòng lỗi không để lại thay đổi nào trong bộ nhớ cho dòng kế tiếp
                    _context.ChangeTracker.Clear();
                }
            }

            if (ketQua.Loi.Count > 0)
                ketQua.FileLoiBase64 = Convert.ToBase64String(TaoFileLoi(ketQua.Loi));

            await _auditLog.LogAsync("Import", "File", tenFile,
                $"Thêm mới: {ketQua.SoThemMoi}, cập nhật: {ketQua.SoCapNhat}, lỗi: {ketQua.SoLoi}");
            await _context.SaveChangesAsync();

            return ketQua;
        }

        // ===================== Kiểm tra file =====================

        private static XLWorkbook MoFile(Stream file)
        {
            try
            {
                return new XLWorkbook(file);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                throw new NghiepVuException("File không đúng định dạng .xlsx.");
            }
        }

        private static void KiemTraTieuDe(IXLWorksheet ws)
        {
            for (int i = 0; i < CotMau.Length; i++)
            {
                if (ws.Cell(1, i + 1).GetString().Trim() != CotMau[i])
                    throw new NghiepVuException($"File sai mẫu: cột {i + 1} phải là '{CotMau[i]}'.");
            }
            if (ws.Row(1).LastCellUsed()?.Address.ColumnNumber > CotMau.Length)
                throw new NghiepVuException($"File sai mẫu: chỉ có {CotMau.Length} cột theo mẫu BM02.");
        }

        // ===================== Xử lý từng dòng =====================

        private sealed record LoiCot(string Cot, string NoiDung);

        private sealed class DongLoiException : Exception
        {
            public IReadOnlyList<LoiCot> Loi { get; }

            public DongLoiException(IReadOnlyList<LoiCot> loi) : base(loi[0].NoiDung) { Loi = loi; }
            public DongLoiException(string cot, string noiDung) : this(new[] { new LoiCot(cot, noiDung) }) { }
        }

        private static void GhiLoi(ImportResultDto ketQua, int dong, string maSV, IEnumerable<LoiCot> loi)
        {
            ketQua.SoLoi++;
            ketQua.Loi.AddRange(loi.Select(l => new ImportErrorDto
            {
                Dong = dong,
                MaSV = maSV.Length == 0 ? null : maSV,
                Cot = l.Cot,
                NoiDung = l.NoiDung
            }));
        }

        private static string? DocText(IXLWorksheet ws, int r, int cot)
        {
            var s = ws.Cell(r, cot).GetString().Trim();
            return s.Length == 0 ? null : s;
        }

        private static string DocMaSV(IXLWorksheet ws, int r) => (DocText(ws, r, 1) ?? "").ToUpperInvariant();

        // Trả về true nếu thêm mới, false nếu cập nhật
        private async Task<bool> XuLyDongAsync(IXLWorksheet ws, int r, string maSV,
            Dictionary<string, string> lopDangDung, HashSet<string> lopDuocPhep)
        {
            if (maSV.Length == 0) throw new DongLoiException(CotMaSV, "Thiếu mã sinh viên");

            var daXoa = await _context.SinhViens.IgnoreQueryFilters().AsNoTracking()
                .Where(s => s.MaSV == maSV)
                .Select(s => (bool?)s.DaXoa)
                .FirstOrDefaultAsync();
            if (daXoa == true)
                throw new DongLoiException(CotMaSV, "Mã SV trùng với hồ sơ đã xóa mềm, không được dùng lại");
            var themMoi = daXoa == null;

            var loi = new List<LoiCot>();

            var ngaySinh = DocNgay(ws.Cell(r, 3), loi);
            var gioiTinh = DocEnum(DocText(ws, r, 4), Enum.GetValues<GioiTinh>(), EnumExtensions.TenHienThi);
            if (gioiTinh == null) loi.Add(new LoiCot(CotGioiTinh, "Giới tính phải là Nam, Nữ hoặc Khác"));
            var trangThai = DocEnum(DocText(ws, r, 9), Enum.GetValues<TrangThaiHocTap>(), EnumExtensions.TenHienThi);
            if (trangThai == null) loi.Add(new LoiCot(CotTrangThai, "Trạng thái phải là Đang học, Bảo lưu, Đã tốt nghiệp hoặc Thôi học"));

            var maLop = KiemTraLop(DocText(ws, r, 5), lopDangDung, lopDuocPhep, loi);

            var matKhau = DocText(ws, r, 10);
            if (themMoi && matKhau == null) loi.Add(new LoiCot(CotMatKhau, "Dòng thêm mới thiếu mật khẩu ban đầu"));

            // Dùng cùng ViewModel với form nhập tay để quy tắc dữ liệu chỉ có một nơi (mục 3.4)
            var thongTin = new StudentEditVm
            {
                MaSV = maSV,
                HoTen = DocText(ws, r, 2) ?? "",
                NgaySinh = ngaySinh,
                GioiTinh = gioiTinh,
                SDT = DocText(ws, r, 6),
                Email = DocText(ws, r, 7),
                DiaChi = DocText(ws, r, 8)
            };

            if (!themMoi)
            {
                KiemTraVm(thongTin, loi);
                NemNeuCoLoi(loi);
                await _students.UpdateFromImportAsync(thongTin, maLop!, trangThai!.Value);
                return false;
            }

            var hoSoMoi = new StudentCreateVm
            {
                MaSV = maSV,
                HoTen = thongTin.HoTen,
                NgaySinh = ngaySinh,
                GioiTinh = gioiTinh,
                MaLop = maLop ?? "",
                TrangThai = trangThai ?? TrangThaiHocTap.DangHoc,
                SDT = thongTin.SDT,
                Email = thongTin.Email,
                DiaChi = thongTin.DiaChi,
                MatKhau = matKhau ?? ""
            };
            KiemTraVm(hoSoMoi, loi);
            NemNeuCoLoi(loi);
            await _students.CreateAsync(hoSoMoi);
            return true;
        }

        private static void NemNeuCoLoi(List<LoiCot> loi)
        {
            if (loi.Count > 0) throw new DongLoiException(loi);
        }

        private static string? KiemTraLop(string? maLop, Dictionary<string, string> lopDangDung,
            HashSet<string> lopDuocPhep, List<LoiCot> loi)
        {
            if (maLop == null)
            {
                loi.Add(new LoiCot(CotMaLop, "Thiếu mã lớp"));
                return null;
            }
            if (!lopDangDung.TryGetValue(maLop, out var maChuan))
            {
                loi.Add(new LoiCot(CotMaLop, $"Lớp {TextHelper.CatChuoi(maLop, 30)} không tồn tại hoặc đã ngừng sử dụng"));
                return null;
            }
            if (!lopDuocPhep.Contains(maChuan))
            {
                loi.Add(new LoiCot(CotMaLop, $"Lớp {maChuan} không thuộc khoa phụ trách"));
                return null;
            }
            return maChuan;
        }

        private static void KiemTraVm(object vm, List<LoiCot> loi)
        {
            var ketQua = new List<ValidationResult>();
            Validator.TryValidateObject(vm, new ValidationContext(vm), ketQua, validateAllProperties: true);

            foreach (var kq in ketQua)
            {
                foreach (var truong in kq.MemberNames)
                {
                    var cot = truong == nameof(StudentCreateVm.MatKhau) ? CotMatKhau : truong;
                    // Mỗi cột báo một lỗi; lỗi đọc giá trị (ngày, giới tính, lớp...) đã ghi trước đó
                    if (loi.All(l => l.Cot != cot)) loi.Add(new LoiCot(cot, kq.ErrorMessage ?? "Dữ liệu không hợp lệ"));
                }
            }
        }

        // BM02: ô kiểu ngày hoặc chuỗi dd/MM/yyyy
        private static DateTime? DocNgay(IXLCell cell, List<LoiCot> loi)
        {
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime().Date;

            var s = cell.GetString().Trim();
            if (s.Length == 0)
            {
                loi.Add(new LoiCot(CotNgaySinh, "Thiếu ngày sinh"));
                return null;
            }
            if (DateTime.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;

            loi.Add(new LoiCot(CotNgaySinh, $"Ngày sinh \"{TextHelper.CatChuoi(s, 30)}\" không hợp lệ"));
            return null;
        }

        // Giá trị theo đúng tên hiển thị trong BM02, không phân biệt hoa thường
        private static T? DocEnum<T>(string? text, T[] giaTri, Func<T, string> tenHienThi) where T : struct
        {
            if (text == null) return null;
            var chuan = text.Normalize(NormalizationForm.FormC);
            foreach (var v in giaTri)
                if (string.Equals(tenHienThi(v), chuan, StringComparison.OrdinalIgnoreCase)) return v;
            return null;
        }

        // ===================== File danh sách lỗi (BM05) =====================

        private static byte[] TaoFileLoi(List<ImportErrorDto> loi)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Loi");
            string[] cot = { "Dòng", "Mã SV", "Cột", "Nội dung lỗi" };
            for (int c = 0; c < cot.Length; c++)
                ws.Cell(1, c + 1).Value = cot[c];
            ws.Row(1).Style.Font.Bold = true;

            for (int i = 0; i < loi.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = loi[i].Dong;
                ws.Cell(i + 2, 2).Value = loi[i].MaSV ?? "";
                ws.Cell(i + 2, 3).Value = loi[i].Cot;
                ws.Cell(i + 2, 4).Value = loi[i].NoiDung;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
