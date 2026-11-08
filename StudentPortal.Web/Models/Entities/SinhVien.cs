namespace StudentPortal.Models.Entities
{
    public class SinhVien
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;

        // Họ tên chữ thường, bỏ dấu, đ -> d; Service tự tính khi lưu, phục vụ tìm kiếm không dấu (FR-22)
        public string HoTenKhongDau { get; set; } = string.Empty;

        public DateTime NgaySinh { get; set; }
        public GioiTinh GioiTinh { get; set; }
        public string MaLop { get; set; } = string.Empty;
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public TrangThaiHocTap TrangThai { get; set; } = TrangThaiHocTap.DangHoc;

        // Bằng true sau lần đổi trạng thái/chuyển lớp đầu tiên; dùng cho điều kiện xóa của Giáo vụ (FR-18)
        public bool DaCoBienDong { get; set; }

        public bool DaXoa { get; set; }
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }
        public string? LyDoXoa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public DateTime? NgayCapNhat { get; set; }

        public virtual Lop? Lop { get; set; }
        public virtual ApplicationUser? TaiKhoan { get; set; }
    }
}
