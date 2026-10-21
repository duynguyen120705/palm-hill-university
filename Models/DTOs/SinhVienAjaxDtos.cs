using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.DTOs
{
    public class SinhVienCreateDto
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Mã sinh viên từ 6 đến 15 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã sinh viên chỉ chứa chữ và số không dấu")]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime NgaySinh { get; set; }

        public int GioiTinh { get; set; } = 1; // 0: Nữ, 1: Nam, 2: Khác

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public string MaLop { get; set; } = string.Empty;

        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số bắt đầu bằng 0")]
        public string? SDT { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public int TrangThai { get; set; } = 0; // 0: Đang học, 1: Bảo lưu, 2: Thôi học, 3: Đã tốt nghiệp
    }

    public class SinhVienItemDto
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public int GioiTinh { get; set; }
        public string GioiTinhText { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public int TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
        public string NgayTaoFormatted { get; set; } = string.Empty;
    }

    public class PagedResultDto<T>
    {
        public bool Success { get; set; } = true;
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LopOptionDto
    {
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
    }
}

