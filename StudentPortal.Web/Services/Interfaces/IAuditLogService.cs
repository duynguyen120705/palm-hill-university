namespace StudentPortal.Services.Interfaces
{
    // Dịch vụ ghi nhật ký dùng chung (NFR-07, mục 3.2.1.4).
    // Lấy Username từ HttpContext; chỉ Add vào DbContext, được lưu khi bên gọi SaveChanges
    // nên nằm cùng giao dịch với thao tác. Không truyền dữ liệu cá nhân vào chiTiet.
    public interface IAuditLogService
    {
        Task LogAsync(string hanhDong, string doiTuongLoai, string doiTuongMa, string? chiTiet = null);
    }
}
