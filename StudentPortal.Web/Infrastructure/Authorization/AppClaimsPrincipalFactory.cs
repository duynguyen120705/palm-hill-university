using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentPortal.Models.Entities;

namespace StudentPortal.Infrastructure.Authorization
{
    // Thêm MaKhoaPhuTrach, MaSV vào principal để IScopeService đọc phạm vi dữ liệu (FR-09)
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            if (!string.IsNullOrEmpty(user.MaKhoaPhuTrach))
                identity.AddClaim(new Claim(AppClaimTypes.MaKhoaPhuTrach, user.MaKhoaPhuTrach));
            if (!string.IsNullOrEmpty(user.MaSV))
                identity.AddClaim(new Claim(AppClaimTypes.MaSV, user.MaSV));
            return identity;
        }
    }
}
