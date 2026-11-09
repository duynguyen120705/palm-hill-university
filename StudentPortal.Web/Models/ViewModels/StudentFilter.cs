namespace StudentPortal.Models.ViewModels
{
    // Tiêu chí danh sách sinh viên (FR-21, FR-22), dùng chung cho xem danh sách và xuất file (FR-28)
    public class StudentFilter : BoLocSinhVienVm
    {
        // Tìm theo tên hoặc mã SV, không phân biệt hoa thường và dấu (FR-22)
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
