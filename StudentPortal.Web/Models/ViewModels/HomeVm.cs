namespace StudentPortal.Models.ViewModels
{
    // Trang chủ theo vai trò (FR-08): Admin/Giáo vụ có số liệu, Sinh viên có tóm tắt hồ sơ
    public class HomeVm
    {
        public StatisticsRowDto? SoLieu { get; set; }
        public StudentProfileVm? HoSo { get; set; }
    }
}
