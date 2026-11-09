using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Thống kê sinh viên (FR-27); IExportService dùng lại cùng số liệu để xuất file (FR-29)
    public interface IStatisticsService
    {
        Task<StatisticsResultDto> GetAsync(StatisticsFilterVm filter);
    }
}
