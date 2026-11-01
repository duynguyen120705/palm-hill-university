using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    public static class PhanTrangExtensions
    {
        public const int PageSizeToiDa = 100;

        // Phân trang tại CSDL bằng Skip/Take (SQL Server: OFFSET ... FETCH NEXT ...)
        // query phải đã được OrderBy để kết quả giữa các trang ổn định;
        // selector áp dụng sau Skip/Take nên được phép chứa phần định dạng chạy ở client
        public static async Task<PagedResultDto<TResult>> ToPagedResultAsync<TSource, TResult>(
            this IQueryable<TSource> query, int page, int pageSize, Expression<Func<TSource, TResult>> selector)
        {
            if (pageSize < 1 || pageSize > PageSizeToiDa) pageSize = 10;

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return new PagedResultDto<TResult>
            {
                Success = true,
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}
