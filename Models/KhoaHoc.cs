using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("KHOA_HOC")]
    public class KhoaHoc
    {
        [Key]
        [StringLength(20, ErrorMessage = "Mã khóa học tối đa 20 ký tự")]
        [Display(Name = "Mã Khóa Học")]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm nhập học không được để trống")]
        [Range(1900, 2200, ErrorMessage = "Năm nhập học phải trong khoảng 1900 - 2200")]
        [Display(Name = "Năm Nhập Học")]
        public int NamNhapHoc { get; set; }

        [Display(Name = "Đã Xóa")]
        public bool DaXoa { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
    }
}

