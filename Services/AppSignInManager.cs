using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    // Bổ sung kiểm tra TrangThaiTaiKhoan cho Identity (FR-06, FR-08):
    // - Tài khoản BiKhoa không đăng nhập được
    // - Mỗi request xác thực lại SecurityStamp (ValidationInterval = 0); tài khoản bị khóa
    //   hoặc đổi SecurityStamp bị đăng xuất từ yêu cầu kế tiếp, vai trò được nạp lại từ CSDL
    public class AppSignInManager : SignInManager<ApplicationUser>
    {
        public AppSignInManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<bool> CanSignInAsync(ApplicationUser user)
        {
            return user.TrangThaiTaiKhoan == ITaiKhoanService.HoatDong && await base.CanSignInAsync(user);
        }

        public override async Task<ApplicationUser?> ValidateSecurityStampAsync(ClaimsPrincipal? principal)
        {
            var user = await base.ValidateSecurityStampAsync(principal);
            // Chặn cả trường hợp tài khoản bị khóa mà không đổi SecurityStamp.
            // Không xét khóa tạm do đăng nhập sai để người khác gõ sai mật khẩu không đá phiên hợp lệ ra.
            return user != null && user.TrangThaiTaiKhoan == ITaiKhoanService.HoatDong ? user : null;
        }
    }
}
