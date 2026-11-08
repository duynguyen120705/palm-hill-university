using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // Bộ lọc danh sách tài khoản Admin/Giáo vụ (FR-01)
    public class UserFilterVm
    {
        // Tìm theo tên đăng nhập hoặc họ tên, không phân biệt hoa thường và dấu
        [StringLength(100)]
        public string? Keyword { get; set; }
        public string? VaiTro { get; set; }
        public string? MaKhoa { get; set; }
        public TrangThaiTaiKhoan? TrangThai { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoa { get; set; }
        public TrangThaiTaiKhoan TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
    }

    // FR-02 Tạo tài khoản Admin/Giáo vụ
    public class StaffCreateDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaSo]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Khoa phụ trách")]
        public string? MaKhoaPhuTrach { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MatKhauManh]
        [Display(Name = "Mật khẩu ban đầu")]
        public string MatKhau { get; set; } = string.Empty;
    }

    // FR-03 Cập nhật tài khoản Admin/Giáo vụ
    public class StaffEditVm
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Khoa phụ trách")]
        public string? MaKhoaPhuTrach { get; set; }
    }

    // FR-04 Cấp lại mật khẩu tài khoản Admin/Giáo vụ
    public class ResetPasswordVm
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MatKhauManh]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }

    // FR-05 Khóa/mở khóa tài khoản Admin/Giáo vụ
    public class SetLockVm
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        // Bắt buộc gửi lên, tránh thiếu trường bị hiểu ngầm là false (mở khóa)
        [JsonRequired]
        public bool Khoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }
}
