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

        // FR-28: truy vấn lại toàn bộ kết quả lọc (không phân trang) trong phạm vi được phép
        public async Task<FileXuat?> ExportStudentsAsync(StudentFilter filter, bool pdf)
        {
            var ds = await _studentQuery.BuildQuery(filter)
                .OrderBy(s => s.MaSV)
                .Select(s => new { s.MaSV, s.HoTen, s.NgaySinh, s.GioiTinh, s.MaLop, s.Lop!.TenLop, s.SDT, s.Email, s.DiaChi, s.TrangThai })
                .ToListAsync();
            if (ds.Count == 0) return null;

            string[] cot = { "STT", "Mã SV", "Họ tên", "Ngày sinh", "Giới tính", "Lớp", "SĐT", "Email", "Địa chỉ", "Trạng thái" };
            var dong = ds.Select((s, i) => new object?[]
            {
                i + 1, s.MaSV, s.HoTen, s.NgaySinh.ToString("dd/MM/yyyy"),
                MaTrangThai.TenGioiTinh(s.GioiTinh),
                s.TenLop + " (" + s.MaLop + ")", s.SDT, s.Email, s.DiaChi, MaTrangThai.TenTrangThai(s.TrangThai)
            }).ToList();

            var userName = _scope.GetCurrent().UserName;
            var phamVi = MoTaPhamVi(filter);
            var tieuDe = "DANH SÁCH SINH VIÊN";
            var moTa = $"{phamVi} | Tổng: {ds.Count} sinh viên | Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm} bởi {userName}";
            var tenFile = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmm}";

            var file = TaoFile(pdf, tieuDe, moTa, cot, dong, tenFile);

            // Ghi nhật ký lần xuất: người xuất, phạm vi, thời điểm (không ghi từ khóa vì có thể là dữ liệu cá nhân)
            await _auditLog.LogAsync(pdf ? "XuatDanhSachPdf" : "XuatDanhSachExcel", "DanhSach", phamVi);
            await _context.SaveChangesAsync();
            return file;
        }

        // FR-29: tổng hợp lại số liệu theo tiêu chí trong phạm vi được phép rồi tạo file
        public async Task<FileXuat?> ExportStatisticsAsync(ThongKeFilterDto filter, bool pdf)
        {
            var kq = await _statistics.GetAsync(filter);
            if (kq.Rows.Count == 0) return null;

            string[] cot = { "Khoa", "Khóa", "Lớp", "Đang học", "Bảo lưu", "Đã tốt nghiệp", "Thôi học", "Tổng" };
            var dong = kq.Rows
                .Select(r => new object?[] { r.TenKhoa, r.MaKhoaHoc, r.TenLop + " (" + r.MaLop + ")", r.DangHoc, r.BaoLuu, r.DaTotNghiep, r.ThoiHoc, r.Tong })
                .ToList();
            var t = kq.TongCong;
            dong.Add(new object?[] { "TỔNG CỘNG", "", kq.Rows.Count + " lớp", t.DangHoc, t.BaoLuu, t.DaTotNghiep, t.ThoiHoc, t.Tong });

            var userName = _scope.GetCurrent().UserName;
            var phamVi = MoTaPhamVi(filter);
            var tieuDe = "BÁO CÁO THỐNG KÊ SINH VIÊN";
            var moTa = $"{phamVi} | Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm} bởi {userName}";
            var tenFile = $"ThongKeSinhVien_{DateTime.Now:yyyyMMdd_HHmm}";

            var file = TaoFile(pdf, tieuDe, moTa, cot, dong, tenFile);

            await _auditLog.LogAsync(pdf ? "XuatThongKePdf" : "XuatThongKeExcel", "ThongKe", phamVi);
            await _context.SaveChangesAsync();
            return file;
        }

        private static FileXuat TaoFile(bool pdf, string tieuDe, string moTa, string[] cot, List<object?[]> dong, string tenFile) =>
            pdf
                ? new FileXuat(ReportFileBuilder.TaoPdf(tieuDe, moTa, cot, dong), ReportFileBuilder.PdfContentType, tenFile + ".pdf")
                : new FileXuat(ReportFileBuilder.TaoExcel(tieuDe, moTa, cot, dong), ReportFileBuilder.ExcelContentType, tenFile + ".xlsx");

        // Mô tả phạm vi xuất dùng cho tiêu đề file và nhật ký
        private string MoTaPhamVi(ThongKeFilterDto filter)
        {
            var phan = new List<string> { _scope.GetCurrent().IsAdmin ? "Toàn trường" : "Khoa phụ trách" };
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa)) phan.Add("Khoa " + filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc)) phan.Add("Khóa " + filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop)) phan.Add("Lớp " + filter.MaLop);
            if (filter.TrangThai.HasValue) phan.Add(MaTrangThai.TenTrangThai(filter.TrangThai.Value));
            if (filter is StudentFilter ds && !string.IsNullOrWhiteSpace(ds.Keyword)) phan.Add("có từ khóa");
            return string.Join(", ", phan);
        }
    }
}
