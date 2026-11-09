using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StudentPortal.Infrastructure
{
    public static class TextHelper
    {
        // Collation cho tìm kiếm không phân biệt hoa thường và dấu tiếng Việt (FR-22).
        // Vietnamese_CI_AI coi ư, ơ, ă, â, ê, ô, đ là chữ cái riêng nên "hung" không khớp "Hưng",
        // "do" không khớp "Đỗ". Latin1_General_100_CI_AI bỏ được mọi dấu, kể cả đ.
        public const string CollationKhongDau = "Latin1_General_100_CI_AI";

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        // Bỏ dấu tiếng Việt, chữ thường, đ -> d, gộp khoảng trắng
        public static string BoDau(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var chuan = s.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in chuan)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return Regex.Replace(sb.ToString(), @"\s+", " ", RegexOptions.None, RegexTimeout);
        }

        public static string CatChuoi(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
