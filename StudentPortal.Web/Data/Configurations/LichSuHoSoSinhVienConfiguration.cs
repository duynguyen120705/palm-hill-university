using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Configurations
{
    public class LichSuHoSoSinhVienConfiguration : IEntityTypeConfiguration<LichSuHoSoSinhVien>
    {
        public void Configure(EntityTypeBuilder<LichSuHoSoSinhVien> b)
        {
            b.HasKey(h => h.Id);
            b.Property(h => h.MaSV).HasMaxLength(15).IsUnicode(false).IsRequired();
            b.Property(h => h.Loai).IsRequired();
            b.Property(h => h.GiaTriCu).HasMaxLength(500);
            b.Property(h => h.GiaTriMoi).HasMaxLength(500);
            b.Property(h => h.LyDo).HasMaxLength(500);
            b.Property(h => h.NguoiThucHien).HasMaxLength(256).IsRequired();
            b.Property(h => h.ThoiDiem).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(h => h.SinhVien)
             .WithMany(s => s.LichSuHoSos)
             .HasForeignKey(h => h.MaSV)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(h => h.MaSV);
            b.HasIndex(h => h.ThoiDiem);
        }
    }
}
