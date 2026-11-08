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

    // Mã sinh viên, tên đăng nhập (6–15) và mã danh mục (2–15)
    public sealed class MaSoAttribute : RegexRuleAttribute
    {
        public int Min { get; }
        public int Max { get; }

        public MaSoAttribute(int min = 6, int max = 15)
        {
            Min = min;
            Max = max;
            ErrorMessage = string.Format(ValidationRules.MaSoMessage, min, max);
        }

        protected override string Pattern => ValidationRules.MaSoPattern(Min, Max);
    }

    public sealed class HoTenAttribute : RegexRuleAttribute
    {
        public HoTenAttribute() { ErrorMessage = ValidationRules.HoTenMessage; }
        protected override string Pattern => ValidationRules.HoTenPattern;
    }

    public sealed class SoDienThoaiAttribute : RegexRuleAttribute
    {
        public SoDienThoaiAttribute() { ErrorMessage = ValidationRules.SoDienThoaiMessage; }
        protected override string Pattern => ValidationRules.SoDienThoaiPattern;
    }

    public sealed class MatKhauManhAttribute : RegexRuleAttribute
    {
        public MatKhauManhAttribute() { ErrorMessage = ValidationRules.MatKhauMessage; }
        protected override string Pattern => ValidationRules.MatKhauPattern;
    }

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
