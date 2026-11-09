using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // FR-13 Thêm hồ sơ sinh viên (phiếu BM01)
    public class StudentCreateVm
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [MaSo]
        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationRules.HoTenMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [NgaySinh(MinAge = 15)]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public GioiTinh? GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        [Display(Name = "Lớp hành chính")]
        public string MaLop { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public TrangThaiHocTap TrangThai { get; set; } = TrangThaiHocTap.DangHoc;

        [SoDienThoai]
        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [StringLength(100, ErrorMessage = ValidationRules.EmailMessage)]
        [EmailAddress(ErrorMessage = ValidationRules.EmailMessage)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Mật khẩu ban đầu không được để trống")]
        [MatKhauManh]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu ban đầu")]
        public string MatKhau { get; set; } = string.Empty;
    }
}
