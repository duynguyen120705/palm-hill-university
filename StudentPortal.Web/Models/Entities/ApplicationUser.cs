using Microsoft.AspNetCore.Identity;

namespace StudentPortal.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Bắt buộc với Admin/CanBo; null với SinhVien (lấy từ hồ sơ)
        public string? HoTen { get; set; }

        // Bắt buộc và bằng UserName với SinhVien
        public string? MaSV { get; set; }

        // Bắt buộc với CanBo; null với Admin/SinhVien
        public string? MaKhoaPhuTrach { get; set; }

        public TrangThaiTaiKhoan TrangThaiTaiKhoan { get; set; } = TrangThaiTaiKhoan.HoatDong;
        public string? LyDoKhoa { get; set; }

        // Null khi chưa từng đăng nhập; dùng cho điều kiện xóa của Giáo vụ (FR-18)
        public DateTime? LanDangNhapCuoi { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public virtual SinhVien? SinhVien { get; set; }
        public virtual Khoa? KhoaPhuTrach { get; set; }
    }
}
