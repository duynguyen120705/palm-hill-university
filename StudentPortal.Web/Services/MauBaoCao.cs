namespace StudentPortal.Services
{
    // Nội dung một file theo mẫu BM03/BM04
    public class MauBaoCao
    {
        public required string MaMau { get; init; }
        public required string TieuDe { get; init; }
        public string? TenKhoa { get; init; }
        public required string TieuChi { get; init; }
        public required CotBaoCao[] Cot { get; init; }
        public required List<object?[]> Dong { get; init; }
        public bool DongCuoiLaTong { get; init; }
        public string? TongSo { get; init; }
        public string? GhiChu { get; init; }
        public bool HienSoTrang { get; init; }
        public required string NguoiXuat { get; init; }
        public required DateTime ThoiDiem { get; init; }
        public required string TenFile { get; init; }
    }
}
