using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    public class StudentListItemDto
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public TrangThaiHocTap TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;
    }
}
