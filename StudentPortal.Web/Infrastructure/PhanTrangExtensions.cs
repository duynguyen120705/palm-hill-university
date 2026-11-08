using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Models.ViewModels;

namespace StudentPortal.Infrastructure
{
    public static class PhanTrangExtensions
    {
        // Mục 3.2.1.5: pageSize mặc định 20, tối đa 100
        public const int PageSizeMacDinh = 20;
        public const int PageSizeToiDa = 100;

        // Phân trang tại CSDL bằng Skip/Take; query phải đã được OrderBy để kết quả giữa các trang ổn định;
        // selector áp dụng sau Skip/Take nên được phép chứa phần định dạng chạy ở client
        public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
            this IQueryable<TSource> query, int page, int pageSize, Expression<Func<TSource, TResult>> selector)
        {
            if (pageSize < 1) pageSize = PageSizeMacDinh;
            if (pageSize > PageSizeToiDa) pageSize = PageSizeToiDa;

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}
