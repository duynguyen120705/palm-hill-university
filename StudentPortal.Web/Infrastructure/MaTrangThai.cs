namespace StudentPortal.Infrastructure
{
    // Mã trạng thái và giới tính đang lưu trong CSDL, kèm tên hiển thị
    public static class MaTrangThai
    {
        // Trạng thái tài khoản
        public const int TkBiKhoa = 0, TkHoatDong = 1;

        // Trạng thái học tập
        public const int DangHoc = 0, BaoLuu = 1, ThoiHoc = 2, DaTotNghiep = 3;

        // Giới tính
        public const int Nu = 0, Nam = 1, Khac = 2;

        public static string TenTrangThai(int trangThai) => trangThai switch
        {
            DangHoc => "Đang học",
            BaoLuu => "Bảo lưu",
            ThoiHoc => "Thôi học",
            DaTotNghiep => "Đã tốt nghiệp",
            _ => "Không xác định"
        };

        public static string TenGioiTinh(int gioiTinh) => gioiTinh switch
        {
            Nam => "Nam",
            Nu => "Nữ",
            _ => "Khác"
        };

        public static string TenTrangThaiTaiKhoan(int trangThai) => trangThai == TkHoatDong ? "Hoạt động" : "Bị khóa";
    }
}
