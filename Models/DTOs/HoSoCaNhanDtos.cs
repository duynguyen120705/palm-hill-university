using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentPortal.Models.DTOs
{
    // FR-11 Xem hồ sơ cá nhân
    public class HoSoCaNhanDto
    {
        public string UserName { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string? MaKhoaPhuTrach { get; set; }
        public string? TenKhoaPhuTrach { get; set; }
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;

        // Chỉ có với tài khoản sinh viên
        public HoSoSinhVienDto? SinhVien { get; set; }
    }

    public class HoSoSinhVienDto
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
    public class CapNhatLienHeDto
    {
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
        public string? SDT { get; set; }

        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        public string? DiaChi { get; set; }
    }

    // FR-10 Đổi mật khẩu
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class DoiMatKhauDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới tối thiểu 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu mới phải gồm cả chữ và số")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu mới không khớp")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }
}
