using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    public class UserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoa { get; set; }
        public TrangThaiTaiKhoan TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
    }
}
