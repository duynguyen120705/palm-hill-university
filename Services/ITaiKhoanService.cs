using StudentPortal.Models;

namespace StudentPortal.Services
{
    // Dịch vụ tài khoản dùng chung (FR-13, FR-19, FR-20, FR-30 và FR-04, FR-05).
    // Dùng chung DbContext với bên gọi nên chạy trong giao dịch của bên gọi; lỗi ném NghiepVuException.
    public interface ITaiKhoanService
    {
        // Quy ước theo dữ liệu seed: TrangThaiTaiKhoan 0=Khóa 1=Hoạt động
        const int BiKhoa = 0, HoatDong = 1;

        // Tạo tài khoản sinh viên: Username = MaSV, vai trò SinhVien
        Task<ApplicationUser> TaoTaiKhoanSinhVienAsync(string maSV, string? email, string matKhau, bool khoa = false);

        Task<ApplicationUser?> TimTheoMaSVAsync(string maSV);

        // Đặt mật khẩu mới (quên mật khẩu): kiểm tra chính sách, lưu hash, đổi SecurityStamp
        Task DatLaiMatKhauAsync(ApplicationUser user, string matKhauMoi);

        // Khóa/mở khóa: cập nhật trạng thái, LockoutEnd và SecurityStamp
        Task KhoaAsync(ApplicationUser user);
        Task MoKhoaAsync(ApplicationUser user);
    }
}
