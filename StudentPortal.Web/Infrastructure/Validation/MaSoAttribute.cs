namespace StudentPortal.Infrastructure.Validation
{
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
}
