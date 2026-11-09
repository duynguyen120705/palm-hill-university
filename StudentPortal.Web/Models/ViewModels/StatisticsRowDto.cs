namespace StudentPortal.Models.ViewModels
{
    // Một dòng thống kê: số sinh viên của một nhóm theo từng trạng thái
    public class StatisticsRowDto
    {
        public string Ten { get; set; } = string.Empty;
        public int DangHoc { get; set; }
        public int BaoLuu { get; set; }
        public int ThoiHoc { get; set; }
        public int DaTotNghiep { get; set; }
        public int Tong { get; set; }
    }
}
