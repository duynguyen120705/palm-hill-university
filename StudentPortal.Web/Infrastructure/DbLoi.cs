using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StudentPortal.Infrastructure
{
    public static class DbLoi
    {
        // SQL Server: 2627 vi phạm khóa chính/UNIQUE constraint, 2601 vi phạm unique index.
        // Xảy ra khi hai yêu cầu đồng thời cùng qua bước kiểm tra trùng mã (FR-13)
        public static bool LaTrungKhoa(DbUpdateException ex) =>
            ex.InnerException is SqlException { Number: 2627 or 2601 };
    }
}
