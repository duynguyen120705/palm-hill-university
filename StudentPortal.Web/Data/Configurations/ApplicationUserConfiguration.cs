using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            b.Property(u => u.HoTen).HasMaxLength(100);
            b.Property(u => u.MaSV).HasMaxLength(15).IsUnicode(false);
            b.Property(u => u.MaKhoaPhuTrach).HasMaxLength(15).IsUnicode(false);
            b.Property(u => u.LyDoKhoa).HasMaxLength(255);
            b.Property(u => u.NgayTao).HasDefaultValueSql("GETUTCDATE()");

            // SinhVien 1 - 0..1 ApplicationUser (UserName = MaSV)
            b.HasOne(u => u.SinhVien)
             .WithOne(s => s.TaiKhoan)
             .HasForeignKey<ApplicationUser>(u => u.MaSV)
             .OnDelete(DeleteBehavior.Restrict);

            // MaSV duy nhất khi khác NULL
            b.HasIndex(u => u.MaSV).IsUnique().HasFilter("[MaSV] IS NOT NULL");

            // Khoa phụ trách của Giáo vụ
            b.HasOne(u => u.KhoaPhuTrach)
             .WithMany(k => k.CanBoPhuTrachs)
             .HasForeignKey(u => u.MaKhoaPhuTrach)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
