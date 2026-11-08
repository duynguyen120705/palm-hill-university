namespace StudentPortal.Infrastructure
{
    // Kết quả thao tác của IAccountService (mục 3.2.1.4)
    public class Result
    {
        public bool Succeeded { get; }
        public string? Error { get; }
        public int StatusCode { get; }

        private Result(bool succeeded, string? error, int statusCode)
        {
            Succeeded = succeeded;
            Error = error;
            StatusCode = statusCode;
        }

        public static Result Ok() => new(true, null, StatusCodes.Status200OK);
        public static Result Fail(string error, int statusCode = StatusCodes.Status400BadRequest) => new(false, error, statusCode);

        // Chuyển lỗi thành NghiepVuException cho bên gọi đang dùng luồng ngoại lệ
        public void ThrowIfFailed()
        {
            if (!Succeeded) throw new NghiepVuException(Error ?? "Thao tác thất bại.", StatusCode);
        }
    }
}
