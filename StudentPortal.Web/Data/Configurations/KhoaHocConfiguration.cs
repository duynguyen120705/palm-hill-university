using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class KhoaHocConfiguration : IEntityTypeConfiguration<KhoaHoc>
    {
        public void Configure(EntityTypeBuilder<KhoaHoc> b)
        {
            b.HasKey(kh => kh.MaKhoaHoc);
            b.Property(kh => kh.MaKhoaHoc).HasMaxLength(20).IsUnicode(false);
            b.Property(kh => kh.NamNhapHoc).IsRequired();
            b.Property(kh => kh.DaXoa).HasDefaultValue(false);

            b.HasQueryFilter(kh => !kh.DaXoa);
        }
    }
}
