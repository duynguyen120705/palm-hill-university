using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // Tiêu chí lọc chung của danh sách và thống kê, nhận từ query string
    public class BoLocSinhVienVm
    {
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
        public string? MaLop { get; set; }
        public TrangThaiHocTap? TrangThai { get; set; }
    }
}
