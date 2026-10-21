using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace ReportGenerator
{
    public class DocxBuilder : IDisposable
    {
        private readonly WordprocessingDocument _package;
        private readonly MainDocumentPart _mainPart;
        private readonly Body _body;

        private DocxBuilder(string path)
        {
            _package = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            _mainPart = _package.AddMainDocumentPart();
            _mainPart.Document = new Document();
            _body = new Body();
            _mainPart.Document.AppendChild(_body);

            // Cấu hình lề trang A4 tiêu chuẩn (2cm = 1134 twips)
            var sectionProps = new SectionProperties();
            var pageSize = new PageSize() { Width = 11906U, Height = 16838U }; // A4
            var pageMargin = new PageMargin() { Top = 1134, Bottom = 1134, Left = 1134, Right = 1134 };
            sectionProps.Append(pageSize, pageMargin);
            _body.Append(sectionProps);
        }

        public static DocxBuilder Create(string path) => new DocxBuilder(path);

        public void AddTitle(string title, string subtitle, string meta)
        {
            var pTitle = new Paragraph(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { Before = "240", After = "120" }
            ));
            var rTitle = new Run(
                new RunProperties(new Bold(), new FontSize() { Val = "36" }, new Color() { Val = "1F4E79" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(title)
            );
            pTitle.Append(rTitle);
            _body.Append(pTitle);

            var pSub = new Paragraph(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { After = "120" }
            ));
            var rSub = new Run(
                new RunProperties(new Bold(), new FontSize() { Val = "26" }, new Color() { Val = "2E75B6" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(subtitle)
            );
            pSub.Append(rSub);
            _body.Append(pSub);

            var pMeta = new Paragraph(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { After = "360" }
            ));
            var rMeta = new Run(
                new RunProperties(new Italic(), new FontSize() { Val = "22" }, new Color() { Val = "595959" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(meta)
            );
            pMeta.Append(rMeta);
            _body.Append(pMeta);

            AddHorizontalRule();
        }

        public void AddHeading1(string text)
        {
            var p = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines() { Before = "360", After = "120" }
            ));
            var r = new Run(
                new RunProperties(new Bold(), new FontSize() { Val = "30" }, new Color() { Val = "1F4E79" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(text)
            );
            p.Append(r);
            _body.Append(p);
        }

        public void AddHeading2(string text)
        {
            var p = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines() { Before = "240", After = "80" }
            ));
            var r = new Run(
                new RunProperties(new Bold(), new FontSize() { Val = "24" }, new Color() { Val = "2E75B6" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(text)
            );
            p.Append(r);
            _body.Append(p);
        }

        public void AddParagraph(string text)
        {
            var p = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines() { After = "100", Line = "276", LineRule = LineSpacingRuleValues.Auto }
            ));
            var r = new Run(
                new RunProperties(new FontSize() { Val = "22" }, new RunFonts() { Ascii = "Calibri" }),
                new Text(text)
            );
            p.Append(r);
            _body.Append(p);
        }

        public void AddBullet(string boldLabel, string text)
        {
            var p = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines() { After = "60" },
                new Indentation() { Left = "360" }
            ));
            var rDot = new Run(new RunProperties(new Bold(), new Color() { Val = "1F4E79" }), new Text("• "));
            var rLabel = new Run(new RunProperties(new Bold(), new FontSize() { Val = "22" }, new RunFonts() { Ascii = "Calibri" }), new Text(boldLabel));
            var rText = new Run(new RunProperties(new FontSize() { Val = "22" }, new RunFonts() { Ascii = "Calibri" }), new Text(text));
            p.Append(rDot, rLabel, rText);
            _body.Append(p);
        }

        public void AddHorizontalRule()
        {
            var p = new Paragraph(new ParagraphProperties(
                new ParagraphBorders(new BottomBorder() { Val = BorderValues.Single, Size = 12, Space = 1, Color = "2E75B6" }),
                new SpacingBetweenLines() { After = "240" }
            ));
            _body.Append(p);
        }

        public void AddImage(string imagePath, double maxWidthInches = 6.2)
        {
            if (!File.Exists(imagePath)) return;

            var imagePart = _mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                imagePart.FeedData(stream);
            }

            int imgPixelWidth = 800;
            int imgPixelHeight = 400;
            using (var img = System.Drawing.Image.FromFile(imagePath))
            {
                imgPixelWidth = img.Width;
                imgPixelHeight = img.Height;
            }

            double widthInches = maxWidthInches;
            double heightInches = (widthInches / imgPixelWidth) * imgPixelHeight;

            long cx = (long)(widthInches * 914400L);
            long cy = (long)(heightInches * 914400L);

            string rId = _mainPart.GetIdOfPart(imagePart);
            string picId = Guid.NewGuid().ToString("N").Substring(0, 8);

            var drawing = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = cx, Cy = cy },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties() { Id = 1U, Name = "CodeSnippet_" + picId },
                    new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = 0U, Name = "Code.png" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = rId, CompressionState = A.BlipCompressionValues.Print },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = cx, Cy = cy }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                ) { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U });

            var p = new Paragraph(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { Before = "100", After = "180" }
            ));
            p.Append(new Run(drawing));
            _body.Append(p);
        }

        public void Save()
        {
            _mainPart.Document.Save();
        }

        public void Dispose()
        {
            _package.Dispose();
        }
    }
}

