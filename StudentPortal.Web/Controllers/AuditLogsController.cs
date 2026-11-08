using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-31 Tra cứu nhật ký thao tác - chỉ Admin, chỉ đọc
    [Authorize(Roles = Roles.Admin)]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: /AuditLogs
        public IActionResult Index()
        {
            return View();
        }

        // GET: /AuditLogs/List?tuNgay=&denNgay=&nguoiThucHien=&hanhDong=&doiTuong=&page=
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AuditLogFilter filter) =>
            Ok(ApiResponse.Ok(await _auditLogService.QueryAsync(filter)));
    }
}
