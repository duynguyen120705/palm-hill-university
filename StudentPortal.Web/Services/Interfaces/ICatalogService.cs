using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Danh mục Khoa, Khóa, Lớp (FR-23 – FR-26).
    // Admin quản lý cả ba loại; Giáo vụ xem trong phạm vi khoa phụ trách và chỉ thao tác trên Lớp thuộc khoa đó.
    public interface ICatalogService
    {
        // Lựa chọn cho dropdown (Catalog/Lookup), đã giới hạn theo phạm vi dữ liệu.
        // type: khoa | khoahoc | lop; với lop có thể lọc theo maKhoa, maKhoaHoc
        Task<List<LookupItemDto>> LookupAsync(string type, string? maKhoa, string? maKhoaHoc);

        Task<PagedResult<FacultyListItemDto>> ListFacultiesAsync(CatalogFilterVm filter);
        Task CreateFacultyAsync(FacultyCreateVm dto);
        Task UpdateFacultyAsync(FacultyEditVm dto);
        Task DeleteFacultyAsync(string maKhoa);

        Task<PagedResult<CourseListItemDto>> ListCoursesAsync(CatalogFilterVm filter);
        Task CreateCourseAsync(CourseCreateVm dto);
        Task UpdateCourseAsync(CourseEditVm dto);
        Task DeleteCourseAsync(string maKhoaHoc);

        Task<PagedResult<ClassListItemDto>> ListClassesAsync(CatalogFilterVm filter);
        Task CreateClassAsync(ClassCreateVm dto);
        Task UpdateClassAsync(ClassEditVm dto);
        Task DeleteClassAsync(string maLop);
    }
}
