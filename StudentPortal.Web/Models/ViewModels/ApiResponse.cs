namespace StudentPortal.Models.ViewModels
{
    // Dạng phản hồi JSON chung (mục 3.2.1.5)
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        public static ApiResponse Ok(object? data = null, string message = "") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse Fail(string message, Dictionary<string, string[]>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
