using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // FR-15 Cập nhật hồ sơ: mã SV giữ cố định; lớp đổi theo FR-17, trạng thái theo FR-16
    public class StudentEditVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationRules.HoTenMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [NgaySinh(MinAge = 15)]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public GioiTinh? GioiTinh { get; set; }

        [SoDienThoai]
        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [StringLength(100, ErrorMessage = ValidationRules.EmailMessage)]
        [EmailAddress(ErrorMessage = ValidationRules.EmailMessage)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}
