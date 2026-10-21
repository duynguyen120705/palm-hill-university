using System.Security.Claims;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    // Thống kê sinh viên (FR-27); FR-29 dùng lại cùng dữ liệu để xuất file
    public interface IThongKeService
    {
        Task<ThongKeResultDto> ThongKeAsync(ThongKeFilterDto filter, ClaimsPrincipal user);
        Task<BoLocThongKeDto> LayBoLocAsync(ClaimsPrincipal user);
        // FR-29; trả về null nếu không có dữ liệu để xuất
        Task<FileXuat?> XuatAsync(ThongKeFilterDto filter, bool pdf, ClaimsPrincipal user);
    }
}
