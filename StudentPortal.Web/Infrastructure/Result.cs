namespace StudentPortal.Infrastructure
{
    // Kết quả thao tác của IAccountService (mục 3.2.1.4)
    public class Result
    {
        public bool Succeeded { get; }
        public string? Error { get; }

        private Result(bool succeeded, string? error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public static Result Ok() => new(true, null);
        public static Result Fail(string error) => new(false, error);

        // Chuyển lỗi thành NghiepVuException cho bên gọi đang dùng luồng ngoại lệ
        public void ThrowIfFailed()
        {
            if (!Succeeded) throw new NghiepVuException(Error ?? "Thao tác thất bại.");
        }
    }
}
