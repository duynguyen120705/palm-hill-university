using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string? HoTen { get; set; }

        [StringLength(15)]
        [Display(Name = "Mã Sinh Viên")]
        public string? MaSV { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã Khoa Phụ Trách")]
        public string? MaKhoaPhuTrach { get; set; }

        [Display(Name = "Trạng Thái Tài Khoản")]
        public int TrangThaiTaiKhoan { get; set; } = 1;

        [NotMapped]
        public TrangThaiTaiKhoan TrangThaiTaiKhoanEnum
        {
            get => (TrangThaiTaiKhoan)TrangThaiTaiKhoan;
            set => TrangThaiTaiKhoan = (int)value;
        }

        [Display(Name = "Lần Đăng Nhập Đầu Tiên")]
        public DateTime? LanDangNhapDauTien { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(MaSV))]
        public virtual SinhVien? SinhVien { get; set; }

        [ForeignKey(nameof(MaKhoaPhuTrach))]
        public virtual Khoa? KhoaPhuTrach { get; set; }
    }
}

