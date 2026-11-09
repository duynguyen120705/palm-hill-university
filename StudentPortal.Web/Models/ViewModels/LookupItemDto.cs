namespace StudentPortal.Models.ViewModels
{
    // Một lựa chọn trong dropdown Khoa/Khóa/Lớp
    public class LookupItemDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
    }
}
