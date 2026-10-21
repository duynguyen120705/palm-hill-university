namespace StudentPortal.Services
{
    // Dịch vụ ghi nhật ký dùng chung (NFR-07): chỉ ghi định danh, không ghi mật khẩu/dữ liệu cá nhân
    public interface INhatKyService
    {
        Task GhiAsync(string username, string hanhDong, string doiTuongLoai, string doiTuongMa);
    }
}
