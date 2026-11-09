using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class KhoaConfiguration : IEntityTypeConfiguration<Khoa>
    {
        public void Configure(EntityTypeBuilder<Khoa> b)
        {
            b.ToTable("Khoa");
            b.HasKey(k => k.MaKhoa);
            b.Property(k => k.MaKhoa).HasMaxLength(15).IsUnicode(false);
            b.Property(k => k.TenKhoa).HasMaxLength(100).IsRequired();
            b.Property(k => k.DaXoa).HasDefaultValue(false);
            b.Property(k => k.NgayTao).HasDefaultValueSql("GETUTCDATE()");

            b.HasQueryFilter(k => !k.DaXoa);
        }
    }
}
