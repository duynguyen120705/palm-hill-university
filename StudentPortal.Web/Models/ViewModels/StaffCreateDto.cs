using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-02 Tạo tài khoản Admin/Giáo vụ
    public class StaffCreateDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaSo]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Khoa phụ trách")]
        public string? MaKhoaPhuTrach { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MatKhauManh]
        [Display(Name = "Mật khẩu ban đầu")]
        public string MatKhau { get; set; } = string.Empty;
    }
}
