namespace StudentPortal.Services.Interfaces
{
    // Phạm vi dữ liệu của tài khoản đang đăng nhập
    public sealed record UserScope(string UserName, string? VaiTro, string? MaKhoaPhuTrach, string? MaSV)
    {
        public bool IsAdmin => VaiTro == Infrastructure.Authorization.Roles.Admin;
        public bool IsCanBo => VaiTro == Infrastructure.Authorization.Roles.CanBo;
        public bool IsSinhVien => VaiTro == Infrastructure.Authorization.Roles.SinhVien;
    }
}
