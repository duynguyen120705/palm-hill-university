using System.Security.Claims;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    // Quản lý tài khoản Admin/Giáo vụ (FR-01 – FR-05) - chỉ Admin sử dụng
    public interface IQuanLyTaiKhoanService
    {
        Task<PagedResultDto<TaiKhoanItemDto>> DanhSachAsync(TaiKhoanFilterDto filter);
        Task<List<LuaChonDto>> DanhSachKhoaAsync();
        Task<TaiKhoanItemDto> TaoAsync(TaiKhoanCreateDto dto, ClaimsPrincipal admin);
        Task<TaiKhoanItemDto> CapNhatAsync(TaiKhoanUpdateDto dto, ClaimsPrincipal admin);
        Task DatLaiMatKhauAsync(DatLaiMatKhauDto dto, ClaimsPrincipal admin);
        Task<TaiKhoanItemDto> DoiTrangThaiAsync(DoiTrangThaiTaiKhoanDto dto, ClaimsPrincipal admin);
    }
}
