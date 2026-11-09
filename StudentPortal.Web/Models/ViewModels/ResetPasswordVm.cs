using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-04 Cấp lại mật khẩu tài khoản Admin/Giáo vụ
    public class ResetPasswordVm
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MatKhauManh]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}
