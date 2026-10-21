using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Models;

namespace StudentPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets nghiệp vụ (hỗ trợ cả tên số ít và số nhiều)
        public DbSet<Khoa> Khoa { get; set; } = null!;
        public DbSet<KhoaHoc> KhoaHoc { get; set; } = null!;
        public DbSet<Lop> Lop { get; set; } = null!;
        public DbSet<SinhVien> SinhVien { get; set; } = null!;
        public DbSet<LichSuHoSoSinhVien> LichSuHoSoSinhVien { get; set; } = null!;
        public DbSet<NhatKyThaoTac> NhatKyThaoTac { get; set; } = null!;

        public DbSet<Khoa> Khoas => Khoa;
        public DbSet<KhoaHoc> KhoaHocs => KhoaHoc;
        public DbSet<Lop> Lops => Lop;
        public DbSet<SinhVien> SinhViens => SinhVien;
        public DbSet<LichSuHoSoSinhVien> LichSuHoSoSinhViens => LichSuHoSoSinhVien;
        public DbSet<NhatKyThaoTac> NhatKyThaoTacs => NhatKyThaoTac;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình thực thể Khoa
            builder.Entity<Khoa>(b =>
            {
                b.HasKey(k => k.MaKhoa);
                b.Property(k => k.MaKhoa).HasMaxLength(20).IsUnicode(false);
                b.Property(k => k.TenKhoa).HasMaxLength(100).IsRequired();
                b.Property(k => k.DaXoa).HasDefaultValue(false);

                // Global Query Filter: Lọc bỏ bản ghi đã xóa mềm
                b.HasQueryFilter(k => !k.DaXoa);
            });

            // 2. Cấu hình thực thể KhoaHoc
            builder.Entity<KhoaHoc>(b =>
            {
                b.HasKey(kh => kh.MaKhoaHoc);
                b.Property(kh => kh.MaKhoaHoc).HasMaxLength(20).IsUnicode(false);
                b.Property(kh => kh.NamNhapHoc).IsRequired();
                b.Property(kh => kh.DaXoa).HasDefaultValue(false);

                b.HasQueryFilter(kh => !kh.DaXoa);
            });

            // 3. Cấu hình thực thể Lop
            builder.Entity<Lop>(b =>
            {
                b.HasKey(l => l.MaLop);
                b.Property(l => l.MaLop).HasMaxLength(20).IsUnicode(false);
                b.Property(l => l.TenLop).HasMaxLength(100).IsRequired();
                b.Property(l => l.MaKhoa).HasMaxLength(20).IsUnicode(false).IsRequired();
                b.Property(l => l.MaKhoaHoc).HasMaxLength(20).IsUnicode(false).IsRequired();
                b.Property(l => l.DaXoa).HasDefaultValue(false);

                // Quan hệ N - 1 với Khoa
                b.HasOne(l => l.Khoa)
                 .WithMany(k => k.Lops)
                 .HasForeignKey(l => l.MaKhoa)
                 .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ N - 1 với KhoaHoc
                b.HasOne(l => l.KhoaHoc)
                 .WithMany(kh => kh.Lops)
                 .HasForeignKey(l => l.MaKhoaHoc)
                 .OnDelete(DeleteBehavior.Restrict);

                // Ràng buộc duy nhất tên lớp trong cùng một khoa và khóa
                b.HasIndex(l => new { l.MaKhoa, l.MaKhoaHoc, l.TenLop }).IsUnique();

                b.HasQueryFilter(l => !l.DaXoa);
            });

            // 4. Cấu hình thực thể SinhVien
            builder.Entity<SinhVien>(b =>
            {
                b.HasKey(s => s.MaSV);
                b.Property(s => s.MaSV).HasMaxLength(15).IsUnicode(false);
                b.Property(s => s.HoTen).HasMaxLength(100).IsRequired();
                b.Property(s => s.NgaySinh).HasColumnType("date").IsRequired();
                b.Property(s => s.GioiTinh).IsRequired();
                b.Property(s => s.MaLop).HasMaxLength(20).IsUnicode(false).IsRequired();
                b.Property(s => s.SDT).HasMaxLength(10).IsUnicode(false);
                b.Property(s => s.Email).HasMaxLength(100);
                b.Property(s => s.DiaChi).HasMaxLength(250);
                b.Property(s => s.TrangThai).IsRequired();
                b.Property(s => s.DaXoa).HasDefaultValue(false);
                b.Property(s => s.LyDoXoa).HasMaxLength(500);
                b.Property(s => s.NguoiXoa).HasMaxLength(256);
                b.Property(s => s.NgayTao).HasDefaultValueSql("GETUTCDATE()");
                b.Property(s => s.NgayCapNhat).HasDefaultValueSql("GETUTCDATE()");

                // Concurrency Token
                b.Property(s => s.RowVersion).IsRowVersion();

                // Quan hệ N - 1 với Lop
                b.HasOne(s => s.Lop)
                 .WithMany(l => l.SinhViens)
                 .HasForeignKey(s => s.MaLop)
                 .OnDelete(DeleteBehavior.Restrict);

                // Index tìm kiếm & lọc nhanh
                b.HasIndex(s => s.MaLop);
                b.HasIndex(s => s.HoTen);
                b.HasIndex(s => s.TrangThai);
                b.HasIndex(s => s.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

                // Global Query Filter
                b.HasQueryFilter(s => !s.DaXoa);
            });

            // 5. Cấu hình thực thể ApplicationUser (TAI_KHOAN)
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.HoTen).HasMaxLength(100);
                b.Property(u => u.MaSV).HasMaxLength(15).IsUnicode(false);
                b.Property(u => u.MaKhoaPhuTrach).HasMaxLength(20).IsUnicode(false);

                // Quan hệ 0..1 - 0..1 với SinhVien
                b.HasOne(u => u.SinhVien)
                 .WithOne(s => s.TaiKhoan)
                 .HasForeignKey<ApplicationUser>(u => u.MaSV)
                 .OnDelete(DeleteBehavior.SetNull);

                // Khóa ngoại MaSV là unique có điều kiện (chỉ áp dụng khi khác NULL)
                b.HasIndex(u => u.MaSV).IsUnique().HasFilter("[MaSV] IS NOT NULL");

                // Quan hệ N - 1 với Khoa (Khoa phụ trách của Cán bộ)
                b.HasOne(u => u.KhoaPhuTrach)
                 .WithMany(k => k.CanBoPhuTrachs)
                 .HasForeignKey(u => u.MaKhoaPhuTrach)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. Cấu hình thực thể LichSuHoSoSinhVien
            builder.Entity<LichSuHoSoSinhVien>(b =>
            {
                b.HasKey(h => h.Id);
                b.Property(h => h.MaSV).HasMaxLength(15).IsUnicode(false).IsRequired();
                b.Property(h => h.Loai).IsRequired();
                b.Property(h => h.GiaTriCu).HasMaxLength(500);
                b.Property(h => h.GiaTriMoi).HasMaxLength(500);
                b.Property(h => h.LyDo).HasMaxLength(500);
                b.Property(h => h.NguoiThucHien).HasMaxLength(256).IsRequired();
                b.Property(h => h.ThoiDiem).HasDefaultValueSql("GETUTCDATE()");

                // Quan hệ N - 1 với SinhVien
                b.HasOne(h => h.SinhVien)
                 .WithMany(s => s.LichSuHoSos)
                 .HasForeignKey(h => h.MaSV)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(h => h.MaSV);
                b.HasIndex(h => h.ThoiDiem);
            });

            // 7. Cấu hình thực thể NhatKyThaoTac (Audit Log)
            builder.Entity<NhatKyThaoTac>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.ThoiDiem).HasDefaultValueSql("GETUTCDATE()");
                b.Property(n => n.Username).HasMaxLength(256).IsRequired();
                b.Property(n => n.HanhDong).HasMaxLength(100).IsRequired();
                b.Property(n => n.DoiTuongLoai).HasMaxLength(100).IsRequired();
                b.Property(n => n.DoiTuongMa).HasMaxLength(100).IsRequired();

                b.HasIndex(n => n.ThoiDiem);
                b.HasIndex(n => n.Username);
                b.HasIndex(n => new { n.DoiTuongLoai, n.DoiTuongMa });
            });
        }

        // Tự động cập nhật NgayCapNhat khi lưu thay đổi
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<SinhVien>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.NgayCapNhat = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
