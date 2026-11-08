using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class KhoaHocConfiguration : IEntityTypeConfiguration<KhoaHoc>
    {
        public void Configure(EntityTypeBuilder<KhoaHoc> b)
        {
            b.ToTable("KhoaHoc", t => t.HasCheckConstraint(
                "CK_KhoaHoc_NamNhapHoc", "[NamNhapHoc] BETWEEN 2000 AND YEAR(GETDATE()) + 1"));
            b.HasKey(kh => kh.MaKhoaHoc);
            b.Property(kh => kh.MaKhoaHoc).HasMaxLength(15).IsUnicode(false);
            b.Property(kh => kh.DaXoa).HasDefaultValue(false);
            b.Property(kh => kh.NgayTao).HasDefaultValueSql("GETUTCDATE()");

            b.HasQueryFilter(kh => !kh.DaXoa);
        }
    }
}
