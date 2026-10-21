namespace StudentPortal.Services
{
    // Lỗi nghiệp vụ trả về cho người dùng (HTTP 400), hoặc 404 khi không tìm thấy
    public class NghiepVuException : Exception
    {
        public int StatusCode { get; }

        public NghiepVuException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
