namespace StudentPortal.Models.ViewModels
{
    // Tiêu chí thống kê (FR-27)
    public class StatisticsFilterVm : BoLocSinhVienVm
    {
        public string NhomTheo { get; set; } = NhomThongKe.Lop;
    }
}
