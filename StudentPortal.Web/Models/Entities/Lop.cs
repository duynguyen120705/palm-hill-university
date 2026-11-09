namespace StudentPortal.Models.Entities
{
    public class Lop
    {
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string MaKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public virtual Khoa? Khoa { get; set; }
        public virtual KhoaHoc? KhoaHoc { get; set; }
        public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
    }
}
