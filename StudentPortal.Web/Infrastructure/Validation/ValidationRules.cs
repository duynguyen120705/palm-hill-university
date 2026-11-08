using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StudentPortal.Infrastructure.Validation
{
    // Quy tắc kiểm tra dữ liệu dùng chung (mục 3.2.2.4) cho ViewModel (client + server) và import Excel
    public static class ValidationRules
    {
        public const string HoTenPattern = @"^[^\d]+$";
        public const string SoDienThoaiPattern = @"^0\d{9}$";
        public const string MatKhauPattern = @"^(?=.*[A-Za-z])(?=.*\d).{8,}$";

        public const string MaSoMessage = "Mã gồm {0}–{1} ký tự chữ và số, không dấu, không khoảng trắng.";
        public const string HoTenMessage = "Họ tên bắt buộc, 2–100 ký tự, không chứa chữ số.";
        public const string SoDienThoaiMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.";
        public const string MatKhauMessage = "Mật khẩu tối thiểu 8 ký tự, gồm cả chữ và số.";
        public const string EmailMessage = "Email không đúng định dạng, tối đa 100 ký tự.";

        // Giới hạn thời gian chạy Regex trên dữ liệu người dùng (chống ReDoS)
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(200);

        public static string MaSoPattern(int min, int max) => $"^[A-Za-z0-9]{{{min},{max}}}$";

        public static bool KhopMau(string value, string pattern) =>
            Regex.IsMatch(value, pattern, RegexOptions.None, Timeout);

        public static bool IsMaSo(string? value, int min = 6, int max = 15) =>
            value != null && KhopMau(value, MaSoPattern(min, max));

        public static bool IsHoTen(string? value) =>
            value != null && value.Length is >= 2 and <= 100 && KhopMau(value, HoTenPattern);

        public static bool IsSoDienThoai(string? value) =>
            value != null && KhopMau(value, SoDienThoaiPattern);

        public static bool IsMatKhauManh(string? value) =>
            value != null && KhopMau(value, MatKhauPattern);

        public static bool IsEmail(string? value) =>
            value != null && value.Length <= 100 && new EmailAddressAttribute().IsValid(value);

        // Không ở tương lai và đủ tuổi tối thiểu tại thời điểm nhập
        public static bool IsNgaySinh(DateTime value, int minAge = 15) =>
            value.Date <= DateTime.Today.AddYears(-minAge);
    }
}
