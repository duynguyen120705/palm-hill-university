using System.Security.Claims;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    // FR-10, FR-11, FR-12 - định danh luôn lấy từ tài khoản đăng nhập, không nhận từ client
    public interface IHoSoCaNhanService
    {
        Task<HoSoCaNhanDto> LayHoSoAsync(ClaimsPrincipal user);
        Task<HoSoSinhVienDto> CapNhatLienHeAsync(CapNhatLienHeDto dto, ClaimsPrincipal user);
        Task DoiMatKhauAsync(DoiMatKhauDto dto, ClaimsPrincipal user);
    }
}
