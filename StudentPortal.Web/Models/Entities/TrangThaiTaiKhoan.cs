namespace StudentPortal.Models.Entities
{
    // Lưu trong CSDL dạng tinyint, mã theo mục 3.2.2.1
    public enum TrangThaiTaiKhoan : byte
    {
        HoatDong = 1,
        BiKhoa = 2 // khóa nghiệp vụ, khác LockoutEnd là khóa tạm do đăng nhập sai
    }
}
