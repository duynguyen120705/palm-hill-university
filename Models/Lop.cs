using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("LOP")]
    public class Lop
    {
        [Key]
        [StringLength(20, ErrorMessage = "Mã lớp tối đa 20 ký tự")]
        [Display(Name = "Mã Lớp")]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên lớp tối đa 100 ký tự")]
        [Display(Name = "Tên Lớp")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã Khoa")]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khóa học không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã Khóa Học")]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Display(Name = "Đã Xóa")]
        public bool DaXoa { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(MaKhoa))]
        public virtual Khoa? Khoa { get; set; }

        [ForeignKey(nameof(MaKhoaHoc))]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
    }
}

