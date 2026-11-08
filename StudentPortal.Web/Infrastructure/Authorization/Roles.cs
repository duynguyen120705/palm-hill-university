namespace StudentPortal.Infrastructure.Authorization
{
    // Tên vai trò lưu trong CSDL (mục 3.1). Mọi chỗ kiểm tra vai trò dùng các hằng này.
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string CanBo = "CanBo";
        public const string SinhVien = "SinhVien";

        // Dùng cho [Authorize(Roles = ...)]
        public const string AdminOrCanBo = Admin + "," + CanBo;

        public static string TenHienThi(string? vaiTro) => vaiTro switch
        {
            Admin => "Quản trị viên",
            CanBo => "Giáo vụ",
            SinhVien => "Sinh viên",
            _ => ""
        };
    }
}
