using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    public class CourseCreateVm
    {
        [Required(ErrorMessage = "Mã khóa không được để trống")]
        [MaSo(2, 15)]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm nhập học không được để trống")]
        public int? NamNhapHoc { get; set; }
    }
}
