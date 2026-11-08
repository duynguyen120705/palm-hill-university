using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class PhienNhapConfiguration : IEntityTypeConfiguration<PhienNhap>
    {
        public void Configure(EntityTypeBuilder<PhienNhap> b)
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.ThoiDiem).HasDefaultValueSql("GETUTCDATE()");
            b.Property(p => p.Username).HasMaxLength(256).IsRequired();
            b.Property(p => p.TenFile).HasMaxLength(255).IsRequired();

            b.HasIndex(p => p.ThoiDiem);
            b.HasIndex(p => p.Username);
        }
    }
}
