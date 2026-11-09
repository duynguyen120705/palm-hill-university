using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    public class ClassCreateVm
    {
        [Required(ErrorMessage = "Mã lớp không được để trống")]
        [MaSo(2, 15)]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(50, ErrorMessage = "Tên lớp tối đa 50 ký tự")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khóa")]
        public string MaKhoaHoc { get; set; } = string.Empty;
    }
}
