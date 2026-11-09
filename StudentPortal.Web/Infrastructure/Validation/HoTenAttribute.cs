namespace StudentPortal.Infrastructure.Validation
{
    public sealed class HoTenAttribute : RegexRuleAttribute
    {
        public HoTenAttribute() { ErrorMessage = ValidationRules.HoTenMessage; }
        protected override string Pattern => ValidationRules.HoTenPattern;
    }
}
