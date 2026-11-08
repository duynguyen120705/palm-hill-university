using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class NhatKyThaoTacConfiguration : IEntityTypeConfiguration<NhatKyThaoTac>
    {
        public void Configure(EntityTypeBuilder<NhatKyThaoTac> b)
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
        }
    }
}
