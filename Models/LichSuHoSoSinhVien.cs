using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("LICH_SU_HO_SO_SINH_VIEN")]
    public class LichSuHoSoSinhVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [StringLength(15)]
        [Display(Name = "Mã Sinh Viên")]
        public string MaSV { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Loại Thao Tác")]
        public int Loai { get; set; }

        [NotMapped]
        public LoaiLichSuHoSo LoaiEnum
        {
            get => (LoaiLichSuHoSo)Loai;
            set => Loai = (int)value;
        }

        [StringLength(500)]
        [Display(Name = "Giá Trị Cũ")]
        public string? GiaTriCu { get; set; }

        [StringLength(500)]
        [Display(Name = "Giá Trị Mới")]
        public string? GiaTriMoi { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý Do Thực Hiện")]
        public string? LyDo { get; set; }

        [Required(ErrorMessage = "Người thực hiện không được để trống")]
        [StringLength(256)]
        [Display(Name = "Người Thực Hiện")]
        public string NguoiThucHien { get; set; } = string.Empty;

        [Display(Name = "Thời Điểm Ghi Nhận")]
        public DateTime ThoiDiem { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(MaSV))]
        public virtual SinhVien? SinhVien { get; set; }
    }
}

