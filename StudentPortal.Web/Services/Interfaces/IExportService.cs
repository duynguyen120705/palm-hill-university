using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Xuất danh sách và báo cáo thống kê ra Excel/PDF (FR-28, FR-29).
    // Server luôn truy vấn lại theo tiêu chí trong phạm vi được phép; mỗi lần xuất ghi nhật ký.
    public interface IExportService
    {
        // Trả về null nếu không có dữ liệu để xuất
        Task<FileXuat?> ExportStudentsAsync(StudentFilter filter, bool pdf);
        Task<FileXuat?> ExportStatisticsAsync(StatisticsFilterVm filter, bool pdf);
    }
}
