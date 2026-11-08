namespace StudentPortal.Models.ViewModels
{
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
}
