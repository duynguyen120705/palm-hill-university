using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.ViewModels
{
    public class CourseEditVm
    {
        [Required]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm nhập học không được để trống")]
        public int? NamNhapHoc { get; set; }
    }
}
