using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // Tiêu chí thống kê (FR-27), nhận từ query string
    public class StatisticsFilterVm
    {
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
        public string? MaLop { get; set; }
        public TrangThaiHocTap? TrangThai { get; set; }
    }

    // Một dòng thống kê: số sinh viên của một lớp theo từng trạng thái
    public class StatisticsRowDto
    {
        public string MaKhoa { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public int DangHoc { get; set; }
        public int BaoLuu { get; set; }
        public int ThoiHoc { get; set; }
        public int DaTotNghiep { get; set; }
        public int Tong { get; set; }
    }

    public class StatisticsResultDto
    {
        public List<StatisticsRowDto> Rows { get; set; } = new();
        public StatisticsRowDto TongCong { get; set; } = new();
    }

    // Một lựa chọn trong dropdown Khoa/Khóa/Lớp
    public class LookupItemDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
    }
}
