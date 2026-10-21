namespace StudentPortal.Models.DTOs
{
    // Tiêu chí thống kê (FR-27), nhận từ query string
    public class ThongKeFilterDto
    {
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
        public string? MaLop { get; set; }
        public int? TrangThai { get; set; }
    }

    // Một dòng thống kê: số sinh viên của một lớp theo từng trạng thái
    public class ThongKeRowDto
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

    public class ThongKeResultDto
    {
        public List<ThongKeRowDto> Rows { get; set; } = new List<ThongKeRowDto>();
        public ThongKeRowDto TongCong { get; set; } = new ThongKeRowDto();
    }

    public class LuaChonDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
    }

    // Lựa chọn cho bộ lọc, đã giới hạn theo phạm vi dữ liệu
    public class BoLocThongKeDto
    {
        public List<LuaChonDto> Khoas { get; set; } = new List<LuaChonDto>();
        public List<LuaChonDto> KhoaHocs { get; set; } = new List<LuaChonDto>();
        public List<LuaChonDto> Lops { get; set; } = new List<LuaChonDto>();
    }
}
