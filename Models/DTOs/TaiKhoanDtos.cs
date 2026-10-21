using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.DTOs
{
    // Bộ lọc danh sách tài khoản Admin/Giáo vụ (FR-01)
    public class TaiKhoanFilterDto
    {
        // Tìm theo tên đăng nhập hoặc họ tên, không phân biệt hoa thường và dấu
        [StringLength(100)]
        public string? Keyword { get; set; }
        public string? VaiTro { get; set; }
        public string? MaKhoa { get; set; }
        public int? TrangThai { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class TaiKhoanItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoa { get; set; }
        public int TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
    }

    // FR-02 Tạo tài khoản Admin/Giáo vụ
    public class TaiKhoanCreateDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Họ tên không chứa chữ số")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9._]{3,50}$", ErrorMessage = "Tên đăng nhập 3–50 ký tự: chữ, số, dấu chấm, gạch dưới")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        public string? MaKhoaPhuTrach { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải gồm cả chữ và số")]
        public string MatKhau { get; set; } = string.Empty;

        [Compare(nameof(MatKhau), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }

    // FR-03 Cập nhật tài khoản Admin/Giáo vụ
    public class TaiKhoanUpdateDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Họ tên không chứa chữ số")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        public string? MaKhoaPhuTrach { get; set; }
    }

    // FR-04 Cấp lại mật khẩu tài khoản Admin/Giáo vụ
    public class DatLaiMatKhauDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải gồm cả chữ và số")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }

    // FR-05 Khóa/mở khóa tài khoản Admin/Giáo vụ
    public class DoiTrangThaiTaiKhoanDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        public bool Khoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }
}
