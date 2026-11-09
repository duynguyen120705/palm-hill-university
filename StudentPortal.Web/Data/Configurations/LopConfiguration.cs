using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class LopConfiguration : IEntityTypeConfiguration<Lop>
    {
        public void Configure(EntityTypeBuilder<Lop> b)
        {
            b.ToTable("Lop");
            b.HasKey(l => l.MaLop);
            b.Property(l => l.MaLop).HasMaxLength(15).IsUnicode(false);
            b.Property(l => l.TenLop).HasMaxLength(50).IsRequired();
            b.Property(l => l.MaKhoa).HasMaxLength(15).IsUnicode(false).IsRequired();
            b.Property(l => l.MaKhoaHoc).HasMaxLength(15).IsUnicode(false).IsRequired();
            b.Property(l => l.DaXoa).HasDefaultValue(false);
            b.Property(l => l.NgayTao).HasDefaultValueSql("GETUTCDATE()");

            // Khóa ngoại Restrict: không xóa dây chuyền vì hệ thống chỉ xóa mềm
            b.HasOne(l => l.Khoa)
             .WithMany(k => k.Lops)
             .HasForeignKey(l => l.MaKhoa)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(l => l.KhoaHoc)
             .WithMany(kh => kh.Lops)
             .HasForeignKey(l => l.MaKhoaHoc)
             .OnDelete(DeleteBehavior.Restrict);

            // Tên lớp duy nhất trong cùng Khoa và Khóa
            b.HasIndex(l => new { l.MaKhoa, l.MaKhoaHoc, l.TenLop }).IsUnique();
            b.HasIndex(l => l.MaKhoa);
            b.HasIndex(l => l.MaKhoaHoc);

            b.HasQueryFilter(l => !l.DaXoa);
        }
    }
}
