using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-06 Đăng nhập
    public class LoginVm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(256)]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    // FR-11 Xem hồ sơ cá nhân
    public class ProfileVm
    {
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoaPhuTrach { get; set; }
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;

        // Chỉ có với tài khoản sinh viên
        public StudentProfileVm? SinhVien { get; set; }
    }

    public class StudentProfileVm
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public string GioiTinhText { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public string TrangThaiText { get; set; } = string.Empty;
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
    }

    // FR-12 Cập nhật thông tin liên hệ: chỉ 3 trường, gửi trường khác bị từ chối (HTTP 400)
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ContactEditVm
    {
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
    }

    // FR-10 Đổi mật khẩu
    public class ChangePasswordVm
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MatKhauManh]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu mới không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }
}
