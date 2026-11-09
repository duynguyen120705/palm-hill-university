namespace StudentPortal.Models.ViewModels
{
    // Báo cáo kết quả nhập Excel, mẫu BM05 (FR-30)
    public class ImportResultDto
    {
        public string TenFile { get; set; } = string.Empty;
        public string NguoiNhap { get; set; } = string.Empty;
        public string ThoiDiem { get; set; } = string.Empty;
        public int TongSoDong { get; set; }
        public int SoThemMoi { get; set; }
        public int SoCapNhat { get; set; }
        public int SoLoi { get; set; }
        public List<ImportErrorDto> Loi { get; set; } = new();

        // File danh sách lỗi .xlsx; trả kèm kết quả để không phải lưu trên server
        public string? FileLoiBase64 { get; set; }
    }
}
