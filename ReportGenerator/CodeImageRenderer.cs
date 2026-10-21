using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReportGenerator
{
    public static class CodeImageRenderer
    {
        public static string RenderFile(string filePath, string title, string outDir, int maxLines = 80)
        {
            string code = File.Exists(filePath) ? File.ReadAllText(filePath) : "// File not found: " + filePath;
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length > maxLines)
            {
                var subset = lines.Take(maxLines).ToList();
                subset.Add($"// ... còn tiếp ({lines.Length - maxLines} dòng nữa)");
                code = string.Join("\n", subset);
            }
            return RenderText(code, title, outDir);
        }

        public static string RenderText(string code, string title, string outDir)
        {
            var rawLines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int fontSize = 11;
            using var font = new Font("Consolas", fontSize, FontStyle.Regular);
            using var fontBold = new Font("Consolas", fontSize, FontStyle.Bold);
            using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);

            // Đo kích thước
            int lineHeight = 19;
            int gutterWidth = 48;
            int maxChars = rawLines.Max(l => l.Length);
            int width = Math.Max(760, gutterWidth + (int)(maxChars * 8.2) + 40);
            width = Math.Min(width, 1000); // Giới hạn vừa vặn trang Word
            int titleBarHeight = 32;
            int height = titleBarHeight + (rawLines.Length * lineHeight) + 20;

            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Nền editor
            using var bgBrush = new SolidBrush(Color.FromArgb(30, 30, 30));
            g.FillRectangle(bgBrush, 0, 0, width, height);

            // 2. Title bar
            using var titleBarBrush = new SolidBrush(Color.FromArgb(45, 45, 48));
            g.FillRectangle(titleBarBrush, 0, 0, width, titleBarHeight);

            // Nút cửa sổ (macOS style dots)
            using (var bRed = new SolidBrush(Color.FromArgb(255, 95, 86))) g.FillEllipse(bRed, 12, 10, 11, 11);
            using (var bYellow = new SolidBrush(Color.FromArgb(255, 189, 46))) g.FillEllipse(bYellow, 28, 10, 11, 11);
            using (var bGreen = new SolidBrush(Color.FromArgb(39, 201, 63))) g.FillEllipse(bGreen, 44, 10, 11, 11);

            // Tên file
            using (var titleBrush = new SolidBrush(Color.FromArgb(204, 204, 204)))
            {
                g.DrawString(title, titleFont, titleBrush, 70, 7);
            }

            // 3. Gutter line numbers
            using var gutterBrush = new SolidBrush(Color.FromArgb(37, 37, 38));
            g.FillRectangle(gutterBrush, 0, titleBarHeight, gutterWidth, height - titleBarHeight);
            using var gutterLinePen = new Pen(Color.FromArgb(51, 51, 51));
            g.DrawLine(gutterLinePen, gutterWidth, titleBarHeight, gutterWidth, height);

            // 4. Vẽ code từng dòng
            using var numBrush = new SolidBrush(Color.FromArgb(110, 118, 129));
            using var textBrush = new SolidBrush(Color.FromArgb(212, 212, 212));
            using var keywordBrush = new SolidBrush(Color.FromArgb(86, 156, 214)); // Xanh dương
            using var stringBrush = new SolidBrush(Color.FromArgb(206, 145, 120)); // Cam
            using var commentBrush = new SolidBrush(Color.FromArgb(106, 153, 85)); // Xanh lá
            using var attrBrush = new SolidBrush(Color.FromArgb(78, 201, 176));   // Xanh ngọc

            for (int i = 0; i < rawLines.Length; i++)
            {
                float y = titleBarHeight + 8 + (i * lineHeight);

                // Số dòng
                string numStr = (i + 1).ToString();
                var numSize = g.MeasureString(numStr, font);
                g.DrawString(numStr, font, numBrush, gutterWidth - numSize.Width - 6, y);

                // Nội dung code
                string line = rawLines[i];
                float x = gutterWidth + 12;

                if (line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith(">"))
                {
                    g.DrawString(line, font, commentBrush, x, y);
                }
                else if (line.TrimStart().StartsWith("[") && line.TrimEnd().EndsWith("]"))
                {
                    g.DrawString(line, font, attrBrush, x, y);
                }
                else
                {
                    DrawHighlightedLine(g, line, font, fontBold, textBrush, keywordBrush, stringBrush, attrBrush, x, y);
                }
            }

            // 5. Viền ngoài
            using var borderPen = new Pen(Color.FromArgb(60, 60, 60), 1);
            g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);

            string fileName = "img_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".png";
            string savePath = Path.Combine(outDir, fileName);
            bmp.Save(savePath, ImageFormat.Png);
            return savePath;
        }

        private static void DrawHighlightedLine(Graphics g, string line, Font font, Font fontBold,
            Brush textBrush, Brush keywordBrush, Brush stringBrush, Brush attrBrush, float x, float y)
        {
            var keywords = new HashSet<string> {
                "public", "private", "protected", "class", "get", "set", "string", "int", "long", "bool", "byte",
                "DateTime", "var", "async", "await", "Task", "IActionResult", "return", "null", "new", "override",
                "if", "else", "switch", "using", "namespace", "virtual", "true", "false", "where", "orderby",
                "select", "from", "in", "static", "void", "try", "catch"
            };

            var regex = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""|//.*|\w+|[^\w\s]+|\s+");
            var matches = regex.Matches(line);

            float currentX = x;
            foreach (Match m in matches)
            {
                string val = m.Value;
                Brush b = textBrush;
                if (val.StartsWith("\"")) b = stringBrush;
                else if (keywords.Contains(val)) b = keywordBrush;
                else if (val.StartsWith("[") || val.EndsWith("]")) b = attrBrush;

                g.DrawString(val, font, b, currentX, y);
                currentX += g.MeasureString(val, font).Width - 3.5f;
            }
        }
    }
}

