namespace StudentPortal.Models.ViewModels
{
    // Tiêu chí tra cứu nhật ký thao tác (FR-31)
    public class AuditLogFilter
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? HanhDong { get; set; }

        // Mã đối tượng: mã SV, mã lớp, tên đăng nhập...
        public string? DoiTuong { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AuditLogDto
    {
        public string ThoiDiem { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string HanhDong { get; set; } = string.Empty;
        public string DoiTuongLoai { get; set; } = string.Empty;
        public string DoiTuongMa { get; set; } = string.Empty;
        public string? ChiTiet { get; set; }
    }

    // Trang chủ theo vai trò (FR-08): Admin/Giáo vụ có số liệu, Sinh viên có tóm tắt hồ sơ
    public class HomeVm
    {
        public StatisticsRowDto? SoLieu { get; set; }
        public StudentProfileVm? HoSo { get; set; }
    }
}
