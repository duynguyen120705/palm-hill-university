using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentPortal.Infrastructure.Validation
{
    // Attribute theo mẫu Regex: server kiểm tra bằng ValidationRules, client dùng adapter regex của jQuery Validation.
    // Giá trị rỗng được coi là hợp lệ; trường bắt buộc dùng thêm [Required].
    public abstract class RegexRuleAttribute : ValidationAttribute, IClientModelValidator
    {
        protected abstract string Pattern { get; }

        public override bool IsValid(object? value) =>
            value is not string s || s.Length == 0 || ValidationRules.KhopMau(s, Pattern);

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.TryAdd("data-val", "true");
            context.Attributes.TryAdd("data-val-regex", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
            context.Attributes.TryAdd("data-val-regex-pattern", Pattern);
        }
    }
}
