using System.ComponentModel.DataAnnotations;
using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // FR-16 Đổi trạng thái học tập
    public class ChangeStatusVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        public TrangThaiHocTap? TrangThaiMoi { get; set; }

        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string? LyDo { get; set; }
    }
}
