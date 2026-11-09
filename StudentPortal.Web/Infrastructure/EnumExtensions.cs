using StudentPortal.Models.Entities;

namespace StudentPortal.Infrastructure
{
    // Tên hiển thị tiếng Việt của enum (mục 3.2.1.5)
    public static class EnumExtensions
    {
        public static string TenHienThi(this TrangThaiHocTap value) => value switch
        {
            TrangThaiHocTap.DangHoc => "Đang học",
            TrangThaiHocTap.BaoLuu => "Bảo lưu",
            TrangThaiHocTap.DaTotNghiep => "Đã tốt nghiệp",
            TrangThaiHocTap.ThoiHoc => "Thôi học",
            _ => "Không xác định"
        };

        public static string TenHienThi(this GioiTinh value) => value switch
        {
            GioiTinh.Nam => "Nam",
            GioiTinh.Nu => "Nữ",
            _ => "Khác"
        };

        public static string TenHienThi(this TrangThaiTaiKhoan value) =>
            value == TrangThaiTaiKhoan.HoatDong ? "Hoạt động" : "Bị khóa";
    }
}
