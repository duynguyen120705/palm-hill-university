namespace StudentPortal.Models.Entities
{
    // Nhật ký thao tác nhạy cảm (NFR-07): chỉ thêm, không sửa/xóa qua giao diện
    public class NhatKyThaoTac
    {
        public long Id { get; set; }
        public DateTime ThoiDiem { get; set; } = DateTime.UtcNow;

        // Lưu dạng chuỗi, không khóa ngoại, để giữ nguyên lịch sử
        public string Username { get; set; } = string.Empty;

        public string HanhDong { get; set; } = string.Empty;
        public string DoiTuongLoai { get; set; } = string.Empty;
        public string DoiTuongMa { get; set; } = string.Empty;

        // Thông tin không nhạy cảm: lý do, trạng thái cũ -> mới, tiêu chí xuất, số dòng import
        public string? ChiTiet { get; set; }
    }
}
