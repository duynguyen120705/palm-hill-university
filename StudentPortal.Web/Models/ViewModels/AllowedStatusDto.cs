using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // Trạng thái được phép chuyển tới theo vai trò và trạng thái hiện tại (bảng quy tắc mục 3.1.2)
    public class AllowedStatusDto
    {
        public TrangThaiHocTap Value { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool BatBuocLyDo { get; set; }
    }
}
