using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data
{
    // DbContext dùng chung cho mọi phân hệ (mục 3.2.1.1); cấu hình Fluent API nằm ở Data/Configurations
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Khoa> Khoas { get; set; } = null!;
        public DbSet<KhoaHoc> KhoaHocs { get; set; } = null!;
        public DbSet<Lop> Lops { get; set; } = null!;
        public DbSet<SinhVien> SinhViens { get; set; } = null!;
        public DbSet<NhatKyThaoTac> NhatKyThaoTacs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // Tự động cập nhật NgayCapNhat khi lưu thay đổi
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<SinhVien>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.NgayCapNhat = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
