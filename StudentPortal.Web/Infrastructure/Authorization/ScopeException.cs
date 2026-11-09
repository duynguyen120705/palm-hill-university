namespace StudentPortal.Infrastructure.Authorization
{
    // Truy cập ngoài phạm vi dữ liệu (FR-09) - trả HTTP 403
    public class ScopeException : NghiepVuException
    {
        public ScopeException(string message = "Bạn không có quyền truy cập dữ liệu này.")
            : base(message, StatusCodes.Status403Forbidden)
        {
        }
    }
}
