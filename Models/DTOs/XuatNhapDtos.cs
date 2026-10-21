namespace StudentPortal.Models.DTOs
{
    public class DongLoiDto
    {
        public int Dong { get; set; }
        public string? MaSV { get; set; }
        public string Loi { get; set; } = string.Empty;
    }

    // Kết quả một lần nhập Excel (FR-30)
    public class KetQuaNhapDto
    {
        public long PhienNhapId { get; set; }
        public string TenFile { get; set; } = string.Empty;
        public int SoThemMoi { get; set; }
        public int SoCapNhat { get; set; }
        public int SoLoi { get; set; }
        public List<DongLoiDto> Loi { get; set; } = new List<DongLoiDto>();
    }

    // Tiêu chí danh sách sinh viên cần xuất (FR-28), dùng chung cho xem trước và xuất file
    public class DanhSachFilterDto : ThongKeFilterDto
    {
        // Tìm theo tên hoặc mã SV, không phân biệt hoa thường và dấu (FR-22)
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PhienNhapItemDto
    {
        public long Id { get; set; }
        public string ThoiDiemFormatted { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string TenFile { get; set; } = string.Empty;
        public int SoThemMoi { get; set; }
        public int SoCapNhat { get; set; }
        public int SoLoi { get; set; }
    }
}
