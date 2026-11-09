namespace StudentPortal.Infrastructure.Validation
{
    public sealed class SoDienThoaiAttribute : RegexRuleAttribute
    {
        public SoDienThoaiAttribute() { ErrorMessage = ValidationRules.SoDienThoaiMessage; }
        protected override string Pattern => ValidationRules.SoDienThoaiPattern;
    }
}
