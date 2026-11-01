namespace StudentPortal.Services
{
    // Collation dùng cho tìm kiếm không phân biệt hoa thường và dấu tiếng Việt (FR-22)
    public static class TimKiemKhongDau
    {
        // Vietnamese_CI_AI coi ư, ơ, ă, â, ê, ô, đ là chữ cái riêng nên "hung" không khớp "Hưng",
        // "do" không khớp "Đỗ". Latin1_General_100_CI_AI bỏ được mọi dấu, kể cả đ.
        public const string Collation = "Latin1_General_100_CI_AI";
    }
}
