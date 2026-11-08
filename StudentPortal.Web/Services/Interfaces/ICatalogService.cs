using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Danh mục Khoa, Khóa, Lớp (FR-23 – FR-26)
    public interface ICatalogService
    {
        // Lựa chọn cho dropdown (Catalog/Lookup), đã giới hạn theo phạm vi dữ liệu.
        // type: khoa | khoahoc | lop; với lop có thể lọc theo maKhoa, maKhoaHoc
        Task<List<LookupItemDto>> LookupAsync(string type, string? maKhoa, string? maKhoaHoc);
    }
}
