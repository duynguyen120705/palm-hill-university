using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentPortal.Infrastructure.Validation
{
    public sealed class NgaySinhAttribute : ValidationAttribute, IClientModelValidator
    {
        public int MinAge { get; set; } = 15;

        public override bool IsValid(object? value) =>
            value is not DateTime d || ValidationRules.IsNgaySinh(d, MinAge);

        public override string FormatErrorMessage(string name) =>
            $"Ngày sinh không ở tương lai và đủ {MinAge} tuổi.";

        // Adapter "ngaysinh" đăng ký trong site.js
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.TryAdd("data-val", "true");
            context.Attributes.TryAdd("data-val-ngaysinh", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
            context.Attributes.TryAdd("data-val-ngaysinh-minage", MinAge.ToString());
        }
    }
}
