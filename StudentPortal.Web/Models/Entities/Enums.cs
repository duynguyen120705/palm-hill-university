namespace StudentPortal.Models.Entities
{
    // Lưu trong CSDL dạng tinyint, mã theo mục 3.2.2.1

    public enum GioiTinh : byte
    {
        Nam = 1,
        Nu = 2,
        Khac = 3
    }

    public enum TrangThaiHocTap : byte
    {
        DangHoc = 1,
        BaoLuu = 2,
        DaTotNghiep = 3,
        ThoiHoc = 4
    }

    public enum TrangThaiTaiKhoan : byte
    {
        HoatDong = 1,
        BiKhoa = 2 // khóa nghiệp vụ, khác LockoutEnd là khóa tạm do đăng nhập sai
    }
}
