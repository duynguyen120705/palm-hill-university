namespace StudentPortal.Repositories
{
    // Repository dùng chung cho truy vấn dùng lại (mục 3.2.1.2); dùng chung AppDbContext (scoped)
    // nên mọi thao tác tham gia giao dịch của Service gọi nó
    public interface IRepository<T> where T : class
    {
        // Truy vấn có global query filter (đã loại bản ghi xóa mềm)
        IQueryable<T> Query();

        // Truy vấn chỉ đọc (AsNoTracking) cho danh sách, thống kê
        IQueryable<T> QueryNoTracking();

        // Bao gồm cả bản ghi đã xóa mềm - dùng để kiểm tra trùng mã
        IQueryable<T> QueryIncludingDeleted();

        Task<T?> FindAsync(params object[] keys);
        void Add(T entity);
        Task<int> SaveChangesAsync();
    }
}
