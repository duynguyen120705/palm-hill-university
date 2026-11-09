namespace StudentPortal.Models.ViewModels
{
    // FR-23 Bộ lọc danh mục; MaKhoa, MaKhoaHoc chỉ dùng cho danh mục Lớp
    public class CatalogFilterVm
    {
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
