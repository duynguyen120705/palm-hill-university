using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.ViewModels
{
    public class FacultyEditVm
    {
        [Required]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string TenKhoa { get; set; } = string.Empty;
    }
}
