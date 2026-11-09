using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.ViewModels
{
    // FR-18 Xóa mềm hồ sơ
    public class StudentDeleteVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }
}
