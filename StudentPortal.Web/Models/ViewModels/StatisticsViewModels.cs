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

    // Giá trị của lựa chọn "Nhóm theo" (UC20)
    public static class NhomThongKe
    {
        public const string Khoa = "khoa";
        public const string KhoaHoc = "khoahoc";
        public const string Lop = "lop";
    }

    // Tiêu chí thống kê (FR-27)
    public class StatisticsFilterVm : BoLocSinhVienVm
    {
        public string NhomTheo { get; set; } = NhomThongKe.Lop;
    }

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

    public class StatisticsResultDto
    {
        // Tên cột nhóm: Khoa, Khóa hoặc Lớp
        public string TenNhom { get; set; } = string.Empty;
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
