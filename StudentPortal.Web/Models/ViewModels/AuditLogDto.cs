namespace StudentPortal.Models.ViewModels
{
    public class AuditLogDto
    {
        public string ThoiDiem { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string HanhDong { get; set; } = string.Empty;
        public string DoiTuongLoai { get; set; } = string.Empty;
        public string DoiTuongMa { get; set; } = string.Empty;
        public string? ChiTiet { get; set; }
    }
}
