namespace StudentPortal.Infrastructure
{
    // Lỗi nghiệp vụ trả về cho người dùng (HTTP 400), hoặc 404 khi không tìm thấy
    public class NghiepVuException : Exception
    {
        public int StatusCode { get; }

        // Trường gây lỗi, dùng cho báo cáo nhập Excel BM05
        public string? Truong { get; init; }

        public NghiepVuException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
