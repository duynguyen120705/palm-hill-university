using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-03 Cập nhật tài khoản Admin/Giáo vụ
    public class StaffEditVm
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Khoa phụ trách")]
        public string? MaKhoaPhuTrach { get; set; }
    }
}
