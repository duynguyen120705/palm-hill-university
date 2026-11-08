using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;

namespace StudentPortal.Infrastructure
{
    // Xử lý ngoại lệ tập trung (R14): lỗi nghiệp vụ hoặc ngoài phạm vi trả mã HTTP tương ứng,
    // lỗi hệ thống trả thông báo chung, không lộ stack trace cho người dùng
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NghiepVuException ex) when (!context.Response.HasStarted && (LaAjax(context.Request) || ex is ScopeException))
            {
                if (LaAjax(context.Request))
                    await GhiJsonAsync(context, ex.StatusCode, ex.Message);
                else
                    context.Response.Redirect("/Account/AccessDenied");
            }
            catch (Exception ex) when (!context.Response.HasStarted && LaAjax(context.Request))
            {
                _logger.LogError(ex, "Lỗi hệ thống khi xử lý {Path}", context.Request.Path);
                await GhiJsonAsync(context, StatusCodes.Status500InternalServerError, "Lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        // Request Ajax/JSON nhận mã lỗi kèm JSON thay vì trang HTML
        public static bool LaAjax(HttpRequest request) =>
            request.Headers.XRequestedWith == "XMLHttpRequest" ||
            request.Headers.Accept.ToString().Contains("application/json") ||
            (request.ContentType?.StartsWith("application/json") ?? false);

        private static Task GhiJsonAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(ApiResponse.Fail(message));
        }
    }
}
