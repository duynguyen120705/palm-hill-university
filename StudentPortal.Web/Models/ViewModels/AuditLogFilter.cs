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
}
