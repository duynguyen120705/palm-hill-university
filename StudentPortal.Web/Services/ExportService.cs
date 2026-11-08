using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class ExportService : IExportService
    {
        private readonly AppDbContext _context;
        private readonly IStudentQueryService _studentQuery;
        private readonly IStatisticsService _statistics;
        private readonly IScopeService _scope;
        private readonly IAuditLogService _auditLog;

        public ExportService(AppDbContext context, IStudentQueryService studentQuery, IStatisticsService statistics,
            IScopeService scope, IAuditLogService auditLog)
        {
            _context = context;
            _studentQuery = studentQuery;
            _statistics = statistics;
            _scope = scope;
            _auditLog = auditLog;
        }

        // FR-28, mẫu BM03: truy vấn lại toàn bộ kết quả lọc (không phân trang) trong phạm vi được phép
        public async Task<FileXuat?> ExportStudentsAsync(StudentFilter filter, bool pdf)
        {
            var ds = await _studentQuery.BuildQuery(filter)
                .OrderBy(s => s.MaSV)
                .Select(s => new { s.MaSV, s.HoTen, s.NgaySinh, s.GioiTinh, s.MaLop, s.Lop!.MaKhoa, s.Lop.MaKhoaHoc, s.SDT, s.Email, s.TrangThai })
                .ToListAsync();
            if (ds.Count == 0) return null;

            var tenKhoa = await TenKhoaAsync(filter);
            var tuKhoa = string.IsNullOrWhiteSpace(filter.Keyword) ? "(không)" : filter.Keyword.Trim();
            var tieuChi = string.Join(" – ",
                "Khoa: " + (tenKhoa ?? "Tất cả"),
                "Khóa: " + (BoTrong(filter.MaKhoaHoc) ?? "Tất cả"),
                "Lớp: " + (BoTrong(filter.MaLop) ?? "Tất cả"),
                "Trạng thái: " + (filter.TrangThai?.TenHienThi() ?? "Tất cả"),
                "Từ khóa: " + tuKhoa);

            var thoiDiem = DateTime.Now;
            var file = TaoFile(pdf, new MauBaoCao
            {
                MaMau = "Mẫu BM03",
                TieuDe = "DANH SÁCH SINH VIÊN",
                TenKhoa = tenKhoa,
                TieuChi = tieuChi,
                Cot = new CotBaoCao[]
                {
                    new("STT", 0.6f, CanLe.Giua), new("Mã SV", 1.3f), new("Họ và tên", 2f), new("Ngày sinh", 1.1f, CanLe.Giua),
                    new("Giới tính", 0.7f, CanLe.Giua), new("Lớp", 1f), new("Khoa", 0.8f), new("Khóa", 0.8f, CanLe.Giua),
                    new("SĐT", 1.2f), new("Email", 2.2f), new("Trạng thái", 1.1f)
                },
                Dong = ds.Select((s, i) => new object?[]
                {
                    i + 1, s.MaSV, s.HoTen, s.NgaySinh.ToString("dd/MM/yyyy"), s.GioiTinh.TenHienThi(),
                    s.MaLop, s.MaKhoa, s.MaKhoaHoc, s.SDT, s.Email, s.TrangThai.TenHienThi()
                }).ToList(),
                TongSo = $"Tổng số: {ds.Count} sinh viên",
                HienSoTrang = true,
                NguoiXuat = _scope.GetCurrent().UserName,
                ThoiDiem = thoiDiem,
                TenFile = $"DanhSachSinhVien_{thoiDiem:yyyyMMdd_HHmm}" + DuoiFile(pdf)
            });

            // Ghi nhật ký: người xuất, phạm vi, tiêu chí, thời điểm (không ghi từ khóa vì có thể là dữ liệu cá nhân)
            await _auditLog.LogAsync("XuatDanhSach", "File", file.TenFile, MoTaPhamVi(filter));
            await _context.SaveChangesAsync();
            return file;
        }

        // FR-29, mẫu BM04: tổng hợp lại số liệu theo tiêu chí trong phạm vi được phép rồi tạo file
        public async Task<FileXuat?> ExportStatisticsAsync(StatisticsFilterVm filter, bool pdf)
        {
            var kq = await _statistics.GetAsync(filter);
            if (kq.Rows.Count == 0) return null;

            var tenKhoa = await TenKhoaAsync(filter);
            var tieuChi = new List<string> { "Thống kê theo: " + kq.TenNhom };
            if (tenKhoa != null) tieuChi.Add("Khoa: " + tenKhoa);
            if (BoTrong(filter.MaKhoaHoc) is { } khoaHoc) tieuChi.Add("Khóa: " + khoaHoc);
            if (BoTrong(filter.MaLop) is { } lop) tieuChi.Add("Lớp: " + lop);
            if (filter.TrangThai.HasValue) tieuChi.Add("Trạng thái: " + filter.TrangThai.Value.TenHienThi());

            var dong = kq.Rows
                .Select(r => new object?[] { r.Ten, r.DangHoc, r.BaoLuu, r.DaTotNghiep, r.ThoiHoc, r.Tong })
                .ToList();
            var t = kq.TongCong;
            dong.Add(new object?[] { "Tổng cộng", t.DangHoc, t.BaoLuu, t.DaTotNghiep, t.ThoiHoc, t.Tong });

            var thoiDiem = DateTime.Now;
            var file = TaoFile(pdf, new MauBaoCao
            {
                MaMau = "Mẫu BM04",
                TieuDe = "BÁO CÁO THỐNG KÊ SINH VIÊN",
                TenKhoa = tenKhoa,
                TieuChi = string.Join(" – ", tieuChi),
                Cot = new CotBaoCao[]
                {
                    new(kq.TenNhom, 2f), new("Đang học", 1f, CanLe.Phai), new("Bảo lưu", 1f, CanLe.Phai),
                    new("Đã tốt nghiệp", 1.2f, CanLe.Phai), new("Thôi học", 1f, CanLe.Phai), new("Tổng", 0.9f, CanLe.Phai)
                },
                Dong = dong,
                DongCuoiLaTong = true,
                GhiChu = "Chỉ tính hồ sơ chưa xóa mềm, trong phạm vi dữ liệu của người xuất.",
                NguoiXuat = _scope.GetCurrent().UserName,
                ThoiDiem = thoiDiem,
                TenFile = $"ThongKeSinhVien_{thoiDiem:yyyyMMdd_HHmm}" + DuoiFile(pdf)
            });

            await _auditLog.LogAsync("XuatThongKe", "File", file.TenFile, MoTaPhamVi(filter));
            await _context.SaveChangesAsync();
            return file;
        }

        private static string DuoiFile(bool pdf) => pdf ? ".pdf" : ".xlsx";

        private static string? BoTrong(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static FileXuat TaoFile(bool pdf, MauBaoCao mau) =>
            pdf
                ? new FileXuat(ReportFileBuilder.TaoPdf(mau), ReportFileBuilder.PdfContentType, mau.TenFile)
                : new FileXuat(ReportFileBuilder.TaoExcel(mau), ReportFileBuilder.ExcelContentType, mau.TenFile);

        // Tên khoa của file xuất: khoa đang lọc, nếu không có thì khoa phụ trách của Giáo vụ
        private async Task<string?> TenKhoaAsync(BoLocSinhVienVm filter)
        {
            var maKhoa = BoTrong(filter.MaKhoa) ?? _scope.GetCurrent().MaKhoaPhuTrach;
            if (maKhoa == null) return null;
            return await _context.Khoas.AsNoTracking()
                .Where(k => k.MaKhoa == maKhoa)
                .Select(k => k.TenKhoa)
                .FirstOrDefaultAsync();
        }

        // Mô tả phạm vi và tiêu chí xuất dùng cho nhật ký
        private string MoTaPhamVi(BoLocSinhVienVm filter)
        {
            var phan = new List<string> { _scope.GetCurrent().IsAdmin ? "Toàn trường" : "Khoa phụ trách" };
            if (filter is StatisticsFilterVm tk) phan.Add("Nhóm theo " + tk.NhomTheo);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa)) phan.Add("Khoa " + filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc)) phan.Add("Khóa " + filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop)) phan.Add("Lớp " + filter.MaLop);
            if (filter.TrangThai.HasValue) phan.Add(filter.TrangThai.Value.TenHienThi());
            if (filter is StudentFilter ds && !string.IsNullOrWhiteSpace(ds.Keyword)) phan.Add("có từ khóa");
            return string.Join(", ", phan);
        }
    }
}
