using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StudentPortal.Services
{
    // Tạo file Excel/PDF trong bộ nhớ, không lưu trên server (FR-28, FR-29, NFR-08)
    public static class ReportFileBuilder
    {
        public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PdfContentType = "application/pdf";

        private const string TenTruong = "TRƯỜNG ĐẠI HỌC MỞ HÀ NỘI";

        private static string? DongDonVi(MauBaoCao m) => m.TenKhoa == null ? null : "KHOA " + m.TenKhoa.ToUpperInvariant();
        private static string NhanNguoiXuat(MauBaoCao m) => "Người xuất: " + m.NguoiXuat;
        private static string NhanThoiDiem(MauBaoCao m) => "Thời điểm xuất: " + m.ThoiDiem.ToString("dd/MM/yyyy HH:mm");
        private static string NhanTenFile(MauBaoCao m) => "Tên file: " + m.TenFile;

        public static byte[] TaoExcel(MauBaoCao m)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("BaoCao");
            var soCot = m.Cot.Length;

            // Các dòng tiêu đề trộn ô để không làm rộng cột dữ liệu
            IXLRange Tron(int dong, int tuCot, int denCot, string noiDung)
            {
                var r = ws.Range(dong, tuCot, dong, denCot).Merge();
                r.FirstCell().Value = noiDung;
                return r;
            }

            Tron(1, 1, soCot - 2, TenTruong);
            Tron(1, soCot - 1, soCot, m.MaMau).Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            if (DongDonVi(m) is { } donVi) Tron(2, 1, soCot - 2, donVi).Style.Font.SetBold();

            Tron(4, 1, soCot, m.TieuDe).Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            Tron(5, 1, soCot, m.TieuChi).Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            const int dongTieuDeBang = 7;
            for (int c = 0; c < soCot; c++)
                ws.Cell(dongTieuDeBang, c + 1).Value = m.Cot[c].Ten;
            var header = ws.Range(dongTieuDeBang, 1, dongTieuDeBang, soCot);
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int r = 0; r < m.Dong.Count; r++)
            {
                for (int c = 0; c < soCot; c++)
                {
                    var cell = ws.Cell(dongTieuDeBang + 1 + r, c + 1);
                    if (m.Dong[r][c] is int so) cell.Value = so;
                    else cell.Value = m.Dong[r][c]?.ToString() ?? "";
                    cell.Style.Alignment.Horizontal = m.Cot[c].CanLe switch
                    {
                        CanLe.Giua => XLAlignmentHorizontalValues.Center,
                        CanLe.Phai => XLAlignmentHorizontalValues.Right,
                        _ => XLAlignmentHorizontalValues.Left
                    };
                }
            }

            var dongCuoiBang = dongTieuDeBang + m.Dong.Count;
            if (m.DongCuoiLaTong) ws.Range(dongCuoiBang, 1, dongCuoiBang, soCot).Style.Font.Bold = true;

            var bang = ws.Range(dongTieuDeBang, 1, dongCuoiBang, soCot);
            bang.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            bang.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var dong = dongCuoiBang + 2;
            if (m.TongSo != null) Tron(dong++, 1, soCot, m.TongSo).Style.Font.SetBold();
            if (m.GhiChu != null) Tron(dong++, 1, soCot, m.GhiChu).Style.Font.SetItalic();
            var giua = soCot / 2;
            Tron(dong, 1, giua, NhanNguoiXuat(m));
            Tron(dong++, giua + 1, soCot, NhanThoiDiem(m));
            Tron(dong, 1, soCot, NhanTenFile(m)).Style.Font.SetItalic();

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static IContainer CanChinh(IContainer o, CanLe canLe) => canLe switch
        {
            CanLe.Giua => o.AlignCenter(),
            CanLe.Phai => o.AlignRight(),
            _ => o.AlignLeft()
        };

        public static byte[] TaoPdf(MauBaoCao m)
        {
            return Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                // Arial có đủ glyph tiếng Việt trên Windows
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(TenTruong).FontSize(11);
                            if (DongDonVi(m) is { } donVi) c.Item().Text(donVi).FontSize(11).Bold();
                        });
                        row.AutoItem().Text(m.MaMau).FontSize(11).Italic();
                    });

                    col.Item().PaddingTop(12).AlignCenter().Text(m.TieuDe).FontSize(16).Bold();
                    col.Item().PaddingTop(4).PaddingBottom(10).AlignCenter().Text(m.TieuChi).FontSize(10).Italic();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            foreach (var cot in m.Cot) cd.RelativeColumn(cot.DoRong);
                        });

                        table.Header(h =>
                        {
                            foreach (var cot in m.Cot)
                                h.Cell().Border(0.75f).Padding(3).AlignCenter().AlignMiddle().Text(cot.Ten).Bold();
                        });

                        for (int r = 0; r < m.Dong.Count; r++)
                        {
                            var dam = m.DongCuoiLaTong && r == m.Dong.Count - 1;
                            for (int c = 0; c < m.Cot.Length; c++)
                            {
                                var text = CanChinh(table.Cell().Border(0.5f).Padding(3), m.Cot[c].CanLe)
                                    .Text(m.Dong[r][c]?.ToString() ?? "");
                                if (dam) text.Bold();
                            }
                        }
                    });

                    col.Item().PaddingTop(10).Column(c =>
                    {
                        c.Spacing(4);
                        if (m.TongSo != null) c.Item().Text(m.TongSo).FontSize(11).Bold();
                        if (m.GhiChu != null) c.Item().Text(m.GhiChu).Italic();
                        c.Item().Row(row =>
                        {
                            row.RelativeItem().Text(NhanNguoiXuat(m));
                            row.RelativeItem(2).Text(NhanThoiDiem(m));
                        });
                        c.Item().Text(NhanTenFile(m)).Italic();
                    });
                });

                if (m.HienSoTrang)
                {
                    page.Footer().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.Italic());
                        t.Span("Trang ");
                        t.CurrentPageNumber();
                        t.Span("/");
                        t.TotalPages();
                    });
                }
            })).GeneratePdf();
        }
    }
}
