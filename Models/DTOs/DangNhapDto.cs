using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.DTOs
{
    // FR-06 Đăng nhập
    public class DangNhapDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(256)]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
