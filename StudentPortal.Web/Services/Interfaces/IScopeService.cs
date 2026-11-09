using StudentPortal.Models.Entities;

namespace StudentPortal.Services.Interfaces
{
    // Bộ lọc phạm vi dữ liệu dùng chung (FR-09, mục 3.2.1.4):
    // Admin - toàn trường; Giáo vụ - khoa phụ trách (khoa của sinh viên xác định qua lớp); Sinh viên - hồ sơ của mình.
    // Mọi truy vấn sinh viên/lớp phải đi qua Apply* trước khi lọc, phân trang, xuất.
    public interface IScopeService
    {
        UserScope GetCurrent();

        IQueryable<SinhVien> ApplyStudentScope(IQueryable<SinhVien> query);
        IQueryable<Lop> ApplyClassScope(IQueryable<Lop> query);

        // Ném ScopeException (HTTP 403) nếu đối tượng nằm ngoài phạm vi
        Task EnsureStudentInScopeAsync(string maSV);
        Task EnsureClassInScopeAsync(string maLop);
    }
}
