using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;

namespace StudentPortal.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query() => _context.Set<T>();

        public IQueryable<T> QueryNoTracking() => _context.Set<T>().AsNoTracking();

        public IQueryable<T> QueryIncludingDeleted() => _context.Set<T>().IgnoreQueryFilters();

        public async Task<T?> FindAsync(params object[] keys) => await _context.Set<T>().FindAsync(keys);

        public void Add(T entity) => _context.Set<T>().Add(entity);

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
