using System.Security.Claims;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    // Bộ lọc phạm vi dữ liệu dùng chung (FR-09) - interface do phân hệ Tài khoản chốt
    public interface IPhamViDuLieuService
    {
        Task<IQueryable<SinhVien>> LocSinhVienAsync(IQueryable<SinhVien> query, ClaimsPrincipal user);
        Task<IQueryable<Lop>> LocLopAsync(IQueryable<Lop> query, ClaimsPrincipal user);
    }
}
