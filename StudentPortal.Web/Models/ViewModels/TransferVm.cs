using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.ViewModels
{
    // FR-17 Chuyển lớp
    public class TransferVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn lớp đích")]
        public string MaLopDich { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string? LyDo { get; set; }
    }
}
