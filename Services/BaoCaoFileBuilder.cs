using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StudentPortal.Services
{
    // Tạo file Excel/PDF trong bộ nhớ, không lưu trên server (FR-28, FR-29, NFR-08)
    public static class BaoCaoFileBuilder
    {
        public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PdfContentType = "application/pdf";

        public static byte[] TaoExcel(string tieuDe, string moTa, string[] cot, IReadOnlyList<object?[]> dong)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("BaoCao");

            ws.Cell(1, 1).Value = tieuDe;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = moTa;

            for (int c = 0; c < cot.Length; c++)
                ws.Cell(4, c + 1).Value = cot[c];
            var header = ws.Range(4, 1, 4, cot.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int r = 0; r < dong.Count; r++)
            {
                for (int c = 0; c < cot.Length; c++)
                {
                    var cell = ws.Cell(r + 5, c + 1);
                    if (dong[r][c] is int so) cell.Value = so;
                    else cell.Value = dong[r][c]?.ToString() ?? "";
                }
            }

            ws.Range(4, 1, dong.Count + 4, cot.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(4, 1, dong.Count + 4, cot.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public static byte[] TaoPdf(string tieuDe, string moTa, string[] cot, IReadOnlyList<object?[]> dong)
        {
            return Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                // Arial có đủ glyph tiếng Việt trên Windows
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Text(tieuDe).FontSize(14).Bold();
                    col.Item().Text(moTa).FontColor(Colors.Grey.Darken2);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        foreach (var _ in cot) cd.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        foreach (var ten in cot)
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(ten).Bold();
                    });

                    foreach (var row in dong)
                        foreach (var v in row)
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3)
                                .Text(v?.ToString() ?? "");
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Trang ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            })).GeneratePdf();
        }
    }

    public record FileXuat(byte[] NoiDung, string ContentType, string TenFile);
}
