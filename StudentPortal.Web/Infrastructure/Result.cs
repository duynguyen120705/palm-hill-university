namespace StudentPortal.Infrastructure
{
    // Kết quả thao tác của IAccountService (mục 3.2.1.4)
    public class Result
    {
        public bool Succeeded { get; }
        public string? Error { get; }
        public int StatusCode { get; }

        // Trường gây lỗi, để form báo lỗi tại đúng trường
        public string? Truong { get; }

        private Result(bool succeeded, string? error, int statusCode, string? truong)
        {
            Succeeded = succeeded;
            Error = error;
            StatusCode = statusCode;
            Truong = truong;
        }

        public static Result Ok() => new(true, null, StatusCodes.Status200OK, null);
        public static Result Fail(string error, int statusCode = StatusCodes.Status400BadRequest, string? truong = null) =>
            new(false, error, statusCode, truong);

        // Chuyển lỗi thành NghiepVuException cho bên gọi đang dùng luồng ngoại lệ
        public void ThrowIfFailed()
        {
            if (!Succeeded) throw new NghiepVuException(Error ?? "Thao tác thất bại.", StatusCode) { Truong = Truong };
        }
    }
}
