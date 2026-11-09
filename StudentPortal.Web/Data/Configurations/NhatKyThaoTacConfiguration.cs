using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class NhatKyThaoTacConfiguration : IEntityTypeConfiguration<NhatKyThaoTac>
    {
        public void Configure(EntityTypeBuilder<NhatKyThaoTac> b)
        {
            b.ToTable("NhatKyThaoTac");
            b.HasKey(n => n.Id);
            b.Property(n => n.ThoiDiem).HasDefaultValueSql("GETUTCDATE()");
            b.Property(n => n.Username).HasMaxLength(256).IsRequired();
            b.Property(n => n.HanhDong).HasMaxLength(50).IsUnicode(false).IsRequired();
            b.Property(n => n.DoiTuongLoai).HasMaxLength(30).IsUnicode(false).IsRequired();
            b.Property(n => n.DoiTuongMa).HasMaxLength(256).IsRequired();
            b.Property(n => n.ChiTiet).HasMaxLength(500);

            b.HasIndex(n => n.ThoiDiem).IsDescending();
            b.HasIndex(n => n.Username);
            b.HasIndex(n => n.DoiTuongMa);
        }
    }
}
