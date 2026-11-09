namespace StudentPortal.Infrastructure.Validation
{
    public sealed class MatKhauManhAttribute : RegexRuleAttribute
    {
        public MatKhauManhAttribute() { ErrorMessage = ValidationRules.MatKhauMessage; }
        protected override string Pattern => ValidationRules.MatKhauPattern;
    }
}
