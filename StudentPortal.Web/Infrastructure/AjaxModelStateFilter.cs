using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentPortal.Models.ViewModels;

namespace StudentPortal.Infrastructure
{
    // Request Ajax có dữ liệu không hợp lệ: trả HTTP 400 kèm errors theo từng trường (mục 3.2.1.5)
    public class AjaxModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid || !ExceptionMiddleware.LaAjax(context.HttpContext.Request)) return;

            // Lỗi đọc JSON (gồm gửi trường ngoài quyền sửa) nằm ở khóa "$..." hoặc làm thiếu tham số "dto";
            // không trả chi tiết kỹ thuật cho client
            if (context.ModelState.Keys.Any(k => k.StartsWith('$') || k == "dto"))
            {
                context.Result = new BadRequestObjectResult(
                    ApiResponse.Fail("Dữ liệu gửi lên không hợp lệ hoặc chứa trường không được phép sửa."));
                return;
            }

            var errors = context.ModelState
                .Where(kv => kv.Value!.Errors.Count > 0)
                .ToDictionary(
                    kv => JsonNamingPolicy.CamelCase.ConvertName(kv.Key.Split('.').Last()),
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(ApiResponse.Fail("Dữ liệu không hợp lệ.", errors));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Không cần xử lý sau khi action chạy
        }
    }
}
