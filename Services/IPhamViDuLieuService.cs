using System.Security.Claims;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    // Bộ lọc phạm vi dữ liệu dùng chung (FR-09):
    // Admin - toàn trường; Giáo vụ - khoa phụ trách (khoa của sinh viên xác định qua lớp); Sinh viên - hồ sơ của mình
    public interface IPhamViDuLieuService
    {
        // Áp điều kiện phạm vi vào truy vấn, gọi trước khi lọc/tìm kiếm/phân trang/xuất
        Task<IQueryable<SinhVien>> LocSinhVienAsync(IQueryable<SinhVien> query, ClaimsPrincipal user);
        Task<IQueryable<Lop>> LocLopAsync(IQueryable<Lop> query, ClaimsPrincipal user);

        // Kiểm tra một đối tượng cụ thể thuộc phạm vi (thao tác trên một hồ sơ/lớp)
        Task<bool> CoQuyenVoiSinhVienAsync(string maSV, ClaimsPrincipal user);
        Task<bool> CoQuyenVoiLopAsync(string maLop, ClaimsPrincipal user);

        // null với Admin/Sinh viên
        Task<string?> LayKhoaPhuTrachAsync(ClaimsPrincipal user);
    }
}
