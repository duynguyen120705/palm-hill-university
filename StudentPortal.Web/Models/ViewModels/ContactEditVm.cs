using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-12 Cập nhật thông tin liên hệ: chỉ 3 trường, gửi trường khác bị từ chối (HTTP 400)
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ContactEditVm
    {
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
