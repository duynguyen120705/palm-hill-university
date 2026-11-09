using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    public class FacultyCreateVm
    {
        [Required(ErrorMessage = "Mã khoa không được để trống")]
        [MaSo(2, 15)]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string TenKhoa { get; set; } = string.Empty;
    }
}
