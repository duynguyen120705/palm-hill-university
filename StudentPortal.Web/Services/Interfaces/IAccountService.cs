using StudentPortal.Infrastructure;
using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Dịch vụ tài khoản dùng chung (mục 3.2.1.4). Dùng chung AppDbContext (scoped) nên tham gia
    // giao dịch của bên gọi; khi bên gọi chưa mở giao dịch thì tự mở cho thao tác nhiều bước.
    // Mỗi thao tác nhạy cảm tự ghi nhật ký qua IAuditLogService.
    public interface IAccountService
    {
        // ===== Phương thức chính theo mục 3.2.1.4 =====

        // Tài khoản sinh viên: Username = MaSV, vai trò SinhVien (FR-13, FR-30)
        Task<Result> CreateStudentAccountAsync(string maSV, string password);

        // Tài khoản Admin/Giáo vụ (FR-02)
        Task<Result> CreateStaffAccountAsync(StaffCreateDto dto);

        // Cấp lại mật khẩu khi quên (FR-04, FR-19)
        Task<Result> ResetPasswordAsync(string userName, string newPassword);

        // Khóa/mở khóa kèm lý do (FR-05, FR-16, FR-20); luôn còn ít nhất một Admin hoạt động
        Task<Result> SetLockAsync(string userName, bool locked, string reason);

        // Người dùng tự đổi mật khẩu (FR-10)
        Task<Result> ChangePasswordAsync(string userId, string current, string newPwd);

        // ===== Quản lý tài khoản Admin/Giáo vụ (FR-01, FR-03) =====

        Task<PagedResultDto<TaiKhoanItemDto>> ListStaffAsync(TaiKhoanFilterDto filter);

        // Không tìm thấy hoặc không phải tài khoản Admin/Giáo vụ: HTTP 404
        Task<TaiKhoanItemDto> GetStaffAsync(string id);

        Task UpdateStaffAsync(TaiKhoanUpdateDto dto);

        // ===== Hồ sơ cá nhân của người đang đăng nhập (FR-11, FR-12) =====

        Task<HoSoCaNhanDto> GetProfileAsync();
        Task<HoSoSinhVienDto> UpdateContactAsync(CapNhatLienHeDto dto);
    }
}
