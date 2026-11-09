using Microsoft.AspNetCore.Mvc.Filters;
using StudentPortal.Data;

namespace StudentPortal.Infrastructure
{
    // Mở giao dịch quanh action gọi trực tiếp IAccountService, vì dịch vụ này chỉ tham gia giao dịch
    // của bên gọi, không tự Commit (mục 3.2.1.4) và Controller không được dùng DbContext (Phụ lục B.3).
    // Có lỗi hoặc action báo lỗi vào ModelState thì không commit, giao dịch tự rollback khi giải phóng.
    public sealed class GiaoDichAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            await using var tx = await db.Database.BeginTransactionAsync();

            var ketQua = await next();

            if (ketQua.Exception == null && context.ModelState.IsValid)
                await tx.CommitAsync();
        }
    }
}
