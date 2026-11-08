using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class SinhVienConfiguration : IEntityTypeConfiguration<SinhVien>
    {
        public void Configure(EntityTypeBuilder<SinhVien> b)
        {
            b.ToTable("SinhVien", t => t.HasCheckConstraint(
                "CK_SinhVien_LyDoXoa", "[DaXoa] = 0 OR [LyDoXoa] IS NOT NULL"));
            b.HasKey(s => s.MaSV);
            b.Property(s => s.MaSV).HasMaxLength(15).IsUnicode(false);
            b.Property(s => s.HoTen).HasMaxLength(100).IsRequired();
            b.Property(s => s.HoTenKhongDau).HasMaxLength(100).IsUnicode(false).IsRequired();
            b.Property(s => s.NgaySinh).HasColumnType("date");
            b.Property(s => s.MaLop).HasMaxLength(15).IsUnicode(false).IsRequired();
            b.Property(s => s.SDT).HasMaxLength(10).IsUnicode(false);
            b.Property(s => s.Email).HasMaxLength(100).IsUnicode(false);
            b.Property(s => s.DiaChi).HasMaxLength(255);
            b.Property(s => s.DaCoBienDong).HasDefaultValue(false);
            b.Property(s => s.DaXoa).HasDefaultValue(false);
            b.Property(s => s.NguoiXoa).HasMaxLength(256);
            b.Property(s => s.LyDoXoa).HasMaxLength(500);
            b.Property(s => s.NgayTao).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(s => s.Lop)
             .WithMany(l => l.SinhViens)
             .HasForeignKey(s => s.MaLop)
             .OnDelete(DeleteBehavior.Restrict);

            // Chỉ mục theo mục 3.2.2.3
            b.HasIndex(s => s.MaLop);
            b.HasIndex(s => s.TrangThai);
            b.HasIndex(s => s.DaXoa);
            b.HasIndex(s => s.HoTenKhongDau);

            b.HasQueryFilter(s => !s.DaXoa);
        }
    }
}
