using StudentPortal.Models.Entities;

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

    public class StudentListItemDto
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public TrangThaiHocTap TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;
    }

    public class ImportErrorDto
    {
        public int Dong { get; set; }
        public string? MaSV { get; set; }
        public string Loi { get; set; } = string.Empty;
    }

    // Kết quả một lần nhập Excel (FR-30)
    public class ImportResultDto
    {
        public string TenFile { get; set; } = string.Empty;
        public int SoThemMoi { get; set; }
        public int SoCapNhat { get; set; }
        public int SoLoi { get; set; }
        public List<ImportErrorDto> Loi { get; set; } = new();
    }
}
