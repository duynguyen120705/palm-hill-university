using StudentPortal.Models.ViewModels;

namespace StudentPortal.Services.Interfaces
{
    // Nhập danh sách sinh viên từ Excel (FR-30)
    public interface IImportService
    {
        // File mẫu BM02
        byte[] CreateTemplate();

        // File chỉ đọc trong bộ nhớ; InvalidDataException khi sai mẫu
        Task<KetQuaNhapDto> ImportAsync(Stream file, string tenFile);

        Task<PagedResultDto<PhienNhapItemDto>> ListSessionsAsync(int page, int pageSize);
        Task DeleteSessionAsync(long id);
    }
}
