namespace StudentPortal.Infrastructure.Authorization
{
    // Claim bổ sung lấy từ tài khoản. Principal được tạo lại ở mỗi request
    // (SecurityStampValidator, ValidationInterval = 0) nên đổi khoa phụ trách áp dụng từ yêu cầu kế tiếp.
    public static class AppClaimTypes
    {
        public const string MaKhoaPhuTrach = "MaKhoaPhuTrach";
        public const string MaSV = "MaSV";
    }
}
