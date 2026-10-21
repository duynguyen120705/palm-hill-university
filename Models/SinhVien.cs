using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    [Table("SINH_VIEN")]
    public class SinhVien
    {
        [Key]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Mã sinh viên phải từ 6 đến 15 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã sinh viên chỉ chứa chữ và số không dấu")]
        [Display(Name = "Mã Sinh Viên")]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Sinh")]
        public DateTime NgaySinh { get; set; }

        [Required]
        [Display(Name = "Giới Tính")]
        public int GioiTinh { get; set; } = 1;

        [NotMapped]
        public GioiTinh GioiTinhEnum
        {
            get => (GioiTinh)GioiTinh;
            set => GioiTinh = (int)value;
        }

        [Required(ErrorMessage = "Lớp không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã Lớp")]
        public string MaLop { get; set; } = string.Empty;

        [StringLength(10)]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số bắt đầu bằng 0")]
        [Display(Name = "Số Điện Thoại")]
        public string? SDT { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email Liên Hệ")]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa Chỉ Thường Trú")]
        public string? DiaChi { get; set; }

        [Required]
        [Display(Name = "Trạng Thái Học Tập")]
        public int TrangThai { get; set; } = 0;

        [NotMapped]
        public TrangThaiSinhVien TrangThaiEnum
        {
            get => (TrangThaiSinhVien)TrangThai;
            set => TrangThai = (int)value;
        }

        [Display(Name = "Đã Xóa")]
        public bool DaXoa { get; set; } = false;

        [Display(Name = "Thời Điểm Xóa")]
        public DateTime? NgayXoa { get; set; }

        [StringLength(256)]
        [Display(Name = "Người Thực Hiện Xóa")]
        public string? NguoiXoa { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý Do Xóa")]
        public string? LyDoXoa { get; set; }

        [Display(Name = "Thời Điểm Khởi Tạo")]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Display(Name = "Thời Điểm Cập Nhật Cuối")]
        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(MaLop))]
        public virtual Lop? Lop { get; set; }

        public virtual ApplicationUser? TaiKhoan { get; set; }

        public virtual ICollection<LichSuHoSoSinhVien> LichSuHoSos { get; set; } = new List<LichSuHoSoSinhVien>();
    }
}

