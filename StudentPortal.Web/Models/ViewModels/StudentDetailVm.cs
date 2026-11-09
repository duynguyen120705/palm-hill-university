using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // FR-14 Chi tiết hồ sơ, kèm các thao tác được phép theo vai trò và trạng thái hồ sơ
    public class StudentDetailVm
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public GioiTinh GioiTinh { get; set; }
        public string GioiTinhText { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public TrangThaiHocTap TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;

        public bool CoTheDoiTrangThai { get; set; }
        public bool CoTheXoa { get; set; }
        public bool CoTheCapLaiMatKhau { get; set; }
        public bool CoTheKhoa { get; set; }
        public bool CoTheMoKhoa { get; set; }
    }
}
