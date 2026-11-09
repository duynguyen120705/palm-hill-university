namespace StudentPortal.Models.Entities
{
    public class KhoaHoc
    {
        public string MaKhoaHoc { get; set; } = string.Empty;
        public int NamNhapHoc { get; set; }
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
    }
}
