using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("KHOA")]
    public class Khoa
    {
        [Key]
        [StringLength(20, ErrorMessage = "Mã khoa tối đa 20 ký tự")]
        [Display(Name = "Mã Khoa")]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        [Display(Name = "Tên Khoa")]
        public string TenKhoa { get; set; } = string.Empty;

        [Display(Name = "Đã Xóa")]
        public bool DaXoa { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
        public virtual ICollection<ApplicationUser> CanBoPhuTrachs { get; set; } = new List<ApplicationUser>();
    }
}

