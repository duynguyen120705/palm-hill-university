using System.Security.Claims;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    // Nhập danh sách sinh viên từ Excel (FR-30); xuất file FR-28, FR-29 bổ sung ở Phase 4
    public interface IXuatNhapService
    {
        byte[] TaoFileMau();
        Task<KetQuaNhapDto> NhapExcelAsync(Stream file, string tenFile, ClaimsPrincipal user);
        Task<PagedResultDto<PhienNhapItemDto>> DanhSachPhienNhapAsync(int page, int pageSize, ClaimsPrincipal user);
        Task<PagedResultDto<SinhVienItemDto>> DanhSachSinhVienAsync(DanhSachFilterDto filter, ClaimsPrincipal user);
        // Trả về null nếu không có dữ liệu để xuất
        Task<FileXuat?> XuatDanhSachAsync(DanhSachFilterDto filter, bool pdf, ClaimsPrincipal user);
        Task<(bool Ok, int StatusCode, string Message)> XoaPhienNhapAsync(long id, ClaimsPrincipal user);
    }
}
