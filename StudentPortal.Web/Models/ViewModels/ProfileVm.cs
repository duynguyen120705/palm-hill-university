namespace StudentPortal.Models.ViewModels
{
    // FR-11 Xem hồ sơ cá nhân
    public class ProfileVm
    {
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoaPhuTrach { get; set; }
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;

        // Chỉ có với tài khoản sinh viên
        public StudentProfileVm? SinhVien { get; set; }
    }
}
