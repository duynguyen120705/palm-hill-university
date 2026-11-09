using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentPortal.Models.ViewModels
{
    // FR-20 Khóa/mở khóa tài khoản sinh viên
    public class StudentSetLockVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        // Bắt buộc gửi lên, tránh thiếu trường bị hiểu ngầm là false (mở khóa)
        [JsonRequired]
        public bool Khoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }
}
