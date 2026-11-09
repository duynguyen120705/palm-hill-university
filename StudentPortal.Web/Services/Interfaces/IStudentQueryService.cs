using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Tra cứu danh sách sinh viên (FR-21, FR-22). BuildQuery được IExportService dùng lại
    // để xuất đúng tiêu chí (FR-28).
    public interface IStudentQueryService
    {
        // Áp phạm vi dữ liệu trước, sau đó tiêu chí lọc và từ khóa
        IQueryable<SinhVien> BuildQuery(StudentFilter filter);

        Task<PagedResult<StudentListItemDto>> SearchAsync(StudentFilter filter);
    }
}
