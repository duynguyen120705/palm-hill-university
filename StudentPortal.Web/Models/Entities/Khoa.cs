namespace StudentPortal.Models.Entities
{
    public class Khoa
    {
        public string MaKhoa { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public bool DaXoa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
        public virtual ICollection<ApplicationUser> CanBoPhuTrachs { get; set; } = new List<ApplicationUser>();
    }
}
