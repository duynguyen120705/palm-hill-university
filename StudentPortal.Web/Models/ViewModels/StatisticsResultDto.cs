namespace StudentPortal.Models.ViewModels
{
    public class StatisticsResultDto
    {
        // Tên cột nhóm: Khoa, Khóa hoặc Lớp
        public string TenNhom { get; set; } = string.Empty;
        public List<StatisticsRowDto> Rows { get; set; } = new();
        public StatisticsRowDto TongCong { get; set; } = new();
    }
}
