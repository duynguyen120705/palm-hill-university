namespace StudentPortal.Models.ViewModels
{
    // Danh sách phân trang, nằm trong ApiResponse.Data
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
