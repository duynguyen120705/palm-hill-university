namespace StudentPortal.Models.ViewModels
{
    public class CourseListItemDto
    {
        public string MaKhoaHoc { get; set; } = string.Empty;
        public int NamNhapHoc { get; set; }
        public int SoLop { get; set; }

        // Khóa đã có lớp chứa sinh viên: giữ cố định năm nhập học (FR-25)
        public bool CoSinhVien { get; set; }
    }
}
