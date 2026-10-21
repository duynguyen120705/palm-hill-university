using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("NHAT_KY_THAO_TAC")]
    public class NhatKyThaoTac
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "Thời Điểm")]
        public DateTime ThoiDiem { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        [StringLength(256)]
        [Display(Name = "Tài Khoản")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hành động không được để trống")]
        [StringLength(100)]
        [Display(Name = "Hành Động")]
        public string HanhDong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại đối tượng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Loại Đối Tượng")]
        public string DoiTuongLoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã đối tượng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Mã Đối Tượng")]
        public string DoiTuongMa { get; set; } = string.Empty;
    }
}

