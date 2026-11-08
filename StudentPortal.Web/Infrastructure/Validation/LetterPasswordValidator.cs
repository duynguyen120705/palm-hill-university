using Microsoft.AspNetCore.Identity;
using StudentPortal.Models.Entities;

namespace StudentPortal.Infrastructure.Validation
{
    // Identity không có tùy chọn "bắt buộc chữ cái" (mục 3.2.1.6)
    public class LetterPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            return Task.FromResult(ValidationRules.IsMatKhauManh(password)
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Code = "MatKhauManh", Description = ValidationRules.MatKhauMessage }));
        }
    }
}
