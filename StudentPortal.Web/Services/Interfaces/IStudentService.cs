using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Toàn bộ quy tắc nghiệp vụ hồ sơ và tài khoản sinh viên FR-13 – FR-20 (mục 3.2.1.4).
    public interface IStudentService
    {
        // Hồ sơ và tài khoản được tạo trong cùng một giao dịch
        Task CreateAsync(StudentCreateVm dto);

        // Không tồn tại hoặc đã xóa mềm: HTTP 404; ngoài phạm vi: HTTP 403
        Task<StudentDetailVm> GetDetailAsync(string maSV);

        Task<StudentDetailVm> UpdateAsync(StudentEditVm dto);
        Task<List<AllowedStatusDto>> GetAllowedStatusesAsync(string maSV);
        Task<StudentDetailVm> ChangeStatusAsync(ChangeStatusVm dto);
        Task<StudentDetailVm> TransferAsync(TransferVm dto);
        Task SoftDeleteAsync(StudentDeleteVm dto);
        Task ResetPasswordAsync(StudentResetPasswordVm dto);
        Task<StudentDetailVm> SetLockAsync(StudentSetLockVm dto);
    }
}
