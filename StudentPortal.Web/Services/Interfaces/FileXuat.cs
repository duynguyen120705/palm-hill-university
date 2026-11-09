namespace StudentPortal.Services.Interfaces
{
    // File tạo trong bộ nhớ, trả thẳng cho trình duyệt, không lưu trên server (NFR-08)
    public record FileXuat(byte[] NoiDung, string ContentType, string TenFile);
}
