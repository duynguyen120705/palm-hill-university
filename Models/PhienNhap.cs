using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    // Lịch sử một lần nhập danh sách sinh viên từ Excel (FR-30)
    [Table("PHIEN_NHAP")]
    public class PhienNhap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "Thời Điểm")]
        public DateTime ThoiDiem { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(256)]
        [Display(Name = "Người Nhập")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Tên File")]
        public string TenFile { get; set; } = string.Empty;

        [Display(Name = "Số Dòng Thêm Mới")]
        public int SoThemMoi { get; set; }

        [Display(Name = "Số Dòng Cập Nhật")]
        public int SoCapNhat { get; set; }

        [Display(Name = "Số Dòng Lỗi")]
        public int SoLoi { get; set; }

        // Danh sách lỗi theo dòng, lưu dạng JSON; không chứa mật khẩu
        [Display(Name = "Chi Tiết Lỗi")]
        public string? ChiTietLoi { get; set; }
    }
}
