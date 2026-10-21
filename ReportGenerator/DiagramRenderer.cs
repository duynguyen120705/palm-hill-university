using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace ReportGenerator
{
    public static class DiagramRenderer
    {
        // =========================================================================
        // 1. Render sơ đồ luồng dữ liệu 8 chặng: UI -> JS -> Controller -> EF Core -> DB -> JSON -> JS -> DOM
        // =========================================================================
        public static string RenderDataFlowDiagram(string outDir)
        {
            string outPath = Path.Combine(outDir, "Diagram_DataFlow_Ajax_Lifecycle.png");
            int width = 1100;
            int height = 540;

            using var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Header Banner
            using (var bannerBrush = new SolidBrush(Color.FromArgb(31, 78, 121)))
            {
                g.FillRectangle(bannerBrush, 0, 0, width, 55);
            }
            using var titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
            using var whiteBrush = new SolidBrush(Color.White);
            g.DrawString("CHU TRÌNH LUẬN CHUYỂN DỮ LIỆU AJAX: UI ➔ JS ➔ CONTROLLER ➔ EF CORE ➔ DB ➔ JSON ➔ JS ➔ DOM", titleFont, whiteBrush, new PointF(25, 16));

            // Định nghĩa 8 bước
            string[] steps = new string[]
            {
                "1. User Action / UI\n(Gõ phím / Click nút / Phân trang)",
                "2. JavaScript Event\n(XHR / jQuery $.ajax / Fetch API)",
                "3. HTTP Request\n(GET / POST JSON qua ngầm)",
                "4. ASP.NET Controller\n(Model Binding & Validation)",
                "5. EF Core LINQ\n(Skip/Take, Filter, Query)",
                "6. SQL Server DB\n(T-SQL Execute & Fetch Data)",
                "7. JSON Serialization\n(JsonResult / HTTP 200 OK)",
                "8. DOM Update\n(innerHTML / createElement / FadeOut)"
            };

            Color[] colors = new Color[]
            {
                Color.FromArgb(52, 152, 219),  // 1. Blue
                Color.FromArgb(155, 89, 182), // 2. Purple
                Color.FromArgb(230, 126, 34), // 3. Orange
                Color.FromArgb(46, 204, 113),  // 4. Green
                Color.FromArgb(26, 188, 156),  // 5. Turquoise
                Color.FromArgb(231, 76, 60),   // 6. Red
                Color.FromArgb(241, 196, 15),  // 7. Yellow
                Color.FromArgb(41, 128, 185)   // 8. Dark Blue
            };

            using var fontStep = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var fontDesc = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            using var fontNum = new Font("Segoe UI", 11, FontStyle.Bold);
            using var penArrow = new Pen(Color.FromArgb(120, 144, 156), 2.5f) { CustomEndCap = new AdjustableArrowCap(5, 5) };

            int boxW = 230;
            int boxH = 95;

            // Hàng 1: Bước 1 -> Bước 2 -> Bước 3 -> Bước 4 (Từ trái sang phải)
            int row1Y = 90;
            for (int i = 0; i < 4; i++)
            {
                int x = 30 + i * 265;
                DrawStepBox(g, x, row1Y, boxW, boxH, (i + 1), steps[i], colors[i]);
                if (i < 3)
                {
                    g.DrawLine(penArrow, x + boxW, row1Y + boxH / 2, x + 265, row1Y + boxH / 2);
                }
            }

            // Mũi tên từ hàng 1 xuống hàng 2 (Bước 4 xuống Bước 5)
            g.DrawLine(penArrow, 30 + 3 * 265 + boxW / 2, row1Y + boxH, 30 + 3 * 265 + boxW / 2, 255);

            // Hàng 2: Bước 5 <- Bước 6 <- Bước 7 <- Bước 8 (Từ phải sang trái - chiều phản hồi)
            int row2Y = 260;
            for (int i = 0; i < 4; i++)
            {
                int stepIdx = 7 - i; // 8, 7, 6, 5
                int x = 30 + (3 - i) * 265;
                DrawStepBox(g, x, row2Y, boxW, boxH, stepIdx + 1, steps[stepIdx], colors[stepIdx]);
            }

            // Mũi tên phản hồi hàng 2: 5 -> 6, 6 -> 7, 7 -> 8
            // Bước 5 (phải nhất) -> Bước 6:
            int x5 = 30 + 3 * 265;
            int x6 = 30 + 2 * 265;
            int x7 = 30 + 1 * 265;
            int x8 = 30;

            g.DrawLine(penArrow, x5, row2Y + boxH / 2, x6 + boxW, row2Y + boxH / 2);
            g.DrawLine(penArrow, x6, row2Y + boxH / 2, x7 + boxW, row2Y + boxH / 2);
            g.DrawLine(penArrow, x7, row2Y + boxH / 2, x8 + boxW, row2Y + boxH / 2);

            // Mũi tên khép vòng: từ Bước 8 (DOM Update) quay lại UI
            using var penReturn = new Pen(Color.FromArgb(41, 128, 185), 2.5f) { DashStyle = DashStyle.Dash, CustomEndCap = new AdjustableArrowCap(5, 5) };
            g.DrawLine(penReturn, 30 + boxW / 2, row2Y, 30 + boxW / 2, row1Y + boxH);

            // Bảng chú thích phía dưới
            using var noteBg = new SolidBrush(Color.FromArgb(245, 247, 250));
            g.FillRectangle(noteBg, 30, 395, 1030, 115);
            using var borderPen = new Pen(Color.FromArgb(207, 216, 220), 1);
            g.DrawRectangle(borderPen, 30, 395, 1030, 115);

            using var noteTitleFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var noteTextFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            using var noteHeaderBrush = new SolidBrush(Color.FromArgb(31, 78, 121));
            using var noteBodyBrush = new SolidBrush(Color.FromArgb(55, 71, 79));

            g.DrawString("★ NGUYÊN LÝ CỐT LÕI CỦA CÔNG NGHỆ AJAX TRONG ASP.NET CORE MVC:", noteTitleFont, noteHeaderBrush, 45, 405);
            g.DrawString("• Bất đồng bộ (Asynchronous): Quá trình gửi và nhận dữ liệu diễn ra ở tầng nền (background), không phong tỏa (non-blocking) giao diện người dùng.", noteTextFont, noteBodyBrush, 45, 430);
            g.DrawString("• Tiết kiệm băng thông: Máy chủ chỉ tuần tự hóa (serialize) dữ liệu thô dạng JSON thay vì kết xuất toàn bộ mã HTML/CSS/JS của trang.", noteTextFont, noteBodyBrush, 45, 452);
            g.DrawString("• Cập nhật cục bộ (Partial DOM Update): JavaScript phân tích payload JSON và cập nhật trực tiếp cây DOM, loại bỏ hiện tượng giật/trắng màn hình.", noteTextFont, noteBodyBrush, 45, 474);

            bmp.Save(outPath, ImageFormat.Png);
            return outPath;
        }

        private static void DrawStepBox(Graphics g, int x, int y, int w, int h, int stepNum, string text, Color accentColor)
        {
            // Vẽ hộp bóng mờ
            using var shadowBrush = new SolidBrush(Color.FromArgb(235, 238, 242));
            g.FillRectangle(shadowBrush, x + 3, y + 3, w, h);

            // Vẽ hộp chính
            using var boxBrush = new SolidBrush(Color.White);
            g.FillRectangle(boxBrush, x, y, w, h);
            using var penBox = new Pen(Color.FromArgb(200, 210, 220), 1.5f);
            g.DrawRectangle(penBox, x, y, w, h);

            // Vạch màu phân biệt
            using var accentBrush = new SolidBrush(accentColor);
            g.FillRectangle(accentBrush, x, y, 6, h);

            // Số thứ tự
            using var circleBrush = new SolidBrush(accentColor);
            g.FillEllipse(circleBrush, x + 14, y + 10, 24, 24);
            using var whiteBrush = new SolidBrush(Color.White);
            using var numFont = new Font("Segoe UI", 10, FontStyle.Bold);
            g.DrawString(stepNum.ToString(), numFont, whiteBrush, x + (stepNum >= 10 ? 17 : 21), y + 13);

            // Nội dung văn bản
            using var textFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            using var boldFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(33, 33, 33));

            var parts = text.Split('\n');
            g.DrawString(parts[0], boldFont, textBrush, x + 44, y + 13);
            if (parts.Length > 1)
            {
                using var descBrush = new SolidBrush(Color.FromArgb(90, 100, 110));
                g.DrawString(parts[1], textFont, descBrush, new RectangleF(x + 14, y + 42, w - 24, h - 46));
            }
        }

        // =========================================================================
        // 2. Render sơ đồ so sánh Traditional HTTP Request vs AJAX Asynchronous Request
        // =========================================================================
        public static string RenderComparisonDiagram(string outDir)
        {
            string outPath = Path.Combine(outDir, "Diagram_Comparison_Traditional_vs_Ajax.png");
            int width = 1100;
            int height = 500;

            using var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Header Banner
            using (var bannerBrush = new SolidBrush(Color.FromArgb(41, 128, 185)))
            {
                g.FillRectangle(bannerBrush, 0, 0, width, 55);
            }
            using var titleFont = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            using var whiteBrush = new SolidBrush(Color.White);
            g.DrawString("SO SÁNH MÔ HÌNH: HTTP REQUEST TRUYỀN THỐNG (FULL RELOAD) VS MÔ HÌNH AJAX (PARTIAL UPDATE)", titleFont, whiteBrush, new PointF(25, 17));

            int halfW = 515;

            // Khung bên trái: HTTP Request Truyền Thống
            DrawComparisonPane(g, 25, 75, halfW, 395, "1. HTTP Request Truyền Thống (Classic Web)",
                Color.FromArgb(231, 76, 60),
                new string[] {
                    "Client submit Form hoặc click Hyperlink (a href)",
                    "Trình duyệt đóng băng màn hình, chuyển sang trạng thái Loading (Trắng màn hình)",
                    "Gửi toàn bộ HTTP Request đến Web Server",
                    "Server xử lý nghiệp vụ, truy vấn Database",
                    "Server kết xuất (render) toàn bộ trang HTML mới kèm CSS/JS",
                    "Client nhận mã HTML đồ sộ, hủy bỏ DOM cũ và vẽ lại toàn bộ từ đầu (Flickering)"
                },
                "❌ Hạn chế: Tốn băng thông gấp 10-20 lần, trải nghiệm người dùng gián đoạn, server chịu tải lớn.");

            // Khung bên phải: AJAX Request Hiện Đại
            DrawComparisonPane(g, 560, 75, halfW, 395, "2. Mô hình Tích hợp AJAX (Asynchronous Web)",
                Color.FromArgb(46, 204, 113),
                new string[] {
                    "Người dùng tương tác (gõ phím, click button, chuyển trang)",
                    "JavaScript gửi HTTP Request ngầm (XHR / jQuery / Fetch API) - Không reload trang",
                    "Giao diện người dùng vẫn giữ nguyên trạng thái, hoàn toàn mượt mà",
                    "Server xử lý Controller & EF Core, chỉ trả về dữ liệu thô dạng JSON siêu nhẹ",
                    "Client tiếp nhận JSON, xử lý logic bất đồng bộ (Promise / Callback)",
                    "JavaScript chỉ cập nhật vùng dữ liệu cần thiết trên DOM (Partial DOM Replacement)"
                },
                "✔ Ưu thế: Phản hồi tức thì, tối ưu 85% băng thông mạng, trải nghiệm mượt mà như Desktop App.");

            bmp.Save(outPath, ImageFormat.Png);
            return outPath;
        }

        private static void DrawComparisonPane(Graphics g, int x, int y, int w, int h, string title, Color accentColor, string[] steps, string footerNote)
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(250, 252, 255));
            g.FillRectangle(bgBrush, x, y, w, h);
            using var borderPen = new Pen(Color.FromArgb(220, 228, 235), 1.5f);
            g.DrawRectangle(borderPen, x, y, w, h);

            // Title Bar
            using var titleBarBrush = new SolidBrush(accentColor);
            g.FillRectangle(titleBarBrush, x, y, w, 40);
            using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using var whiteBrush = new SolidBrush(Color.White);
            g.DrawString(title, titleFont, whiteBrush, x + 15, y + 10);

            // Các bước
            using var stepFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            using var textBrush = new SolidBrush(Color.FromArgb(40, 50, 60));
            using var numFont = new Font("Segoe UI", 9f, FontStyle.Bold);

            int startY = y + 55;
            for (int i = 0; i < steps.Length; i++)
            {
                int itemY = startY + i * 46;
                using var itemCircle = new SolidBrush(accentColor);
                g.FillEllipse(itemCircle, x + 20, itemY + 2, 20, 20);
                g.DrawString((i + 1).ToString(), numFont, whiteBrush, x + (i >= 9 ? 23 : 26), itemY + 4);

                g.DrawString(steps[i], stepFont, textBrush, new RectangleF(x + 50, itemY + 2, w - 65, 40));
            }

            // Footer Note
            int footerY = y + h - 55;
            using var footerBg = new SolidBrush(Color.FromArgb(240, 244, 248));
            g.FillRectangle(footerBg, x + 10, footerY, w - 20, 45);
            using var footerFont = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            using var footerBrush = new SolidBrush(accentColor);
            g.DrawString(footerNote, footerFont, footerBrush, new RectangleF(x + 18, footerY + 8, w - 36, 32));
        }

        // =========================================================================
        // 3. Render ảnh mô phỏng Chrome DevTools Network Tab kiểm chứng AJAX
        // =========================================================================
        public static string RenderNetworkInspectionMockup(string outDir)
        {
            string outPath = Path.Combine(outDir, "Evidence_DevTools_Network_Inspection.png");
            int width = 1100;
            int height = 480;

            using var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(36, 36, 36)); // Dark theme DevTools
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // DevTools Top Bar
            using var barBrush = new SolidBrush(Color.FromArgb(48, 48, 48));
            g.FillRectangle(barBrush, 0, 0, width, 36);

            using var tabFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            using var activeTabFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var tabTextBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
            using var activeTextBrush = new SolidBrush(Color.FromArgb(129, 201, 149));

            g.DrawString("Elements", tabFont, tabTextBrush, 20, 9);
            g.DrawString("Console", tabFont, tabTextBrush, 90, 9);
            g.DrawString("Sources", tabFont, tabTextBrush, 160, 9);
            g.DrawString("Network", activeTabFont, activeTextBrush, 230, 9);
            using var activeLinePen = new Pen(Color.FromArgb(129, 201, 149), 2);
            g.DrawLine(activeLinePen, 230, 34, 285, 34);
            g.DrawString("Performance", tabFont, tabTextBrush, 310, 9);
            g.DrawString("Application", tabFont, tabTextBrush, 410, 9);

            // Filter Bar
            using var filterBarBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
            g.FillRectangle(filterBarBrush, 0, 36, width, 30);
            using var filterFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            using var filterActiveBrush = new SolidBrush(Color.FromArgb(66, 133, 244));
            using var white = new SolidBrush(Color.White);

            g.DrawString("Filter: All |", filterFont, tabTextBrush, 20, 43);
            g.FillRectangle(filterActiveBrush, 85, 41, 75, 20);
            g.DrawString("Fetch/XHR", filterFont, white, 92, 43);
            g.DrawString("| JS | CSS | Img | Media | Font | Doc | WS", filterFont, tabTextBrush, 170, 43);

            // Table Header
            using var thBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
            g.FillRectangle(thBrush, 0, 66, width, 28);
            using var thFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            g.DrawString("Name", thFont, tabTextBrush, 20, 72);
            g.DrawString("Status", thFont, tabTextBrush, 360, 72);
            g.DrawString("Type", thFont, tabTextBrush, 430, 72);
            g.DrawString("Initiator", thFont, tabTextBrush, 520, 72);
            g.DrawString("Size", thFont, tabTextBrush, 660, 72);
            g.DrawString("Time", thFont, tabTextBrush, 750, 72);
            g.DrawString("Waterfall", thFont, tabTextBrush, 850, 72);

            // Rows dữ liệu request minh chứng
            string[][] requests = new string[][]
            {
                new string[] { "GetPagedData?page=1&pageSize=10", "200 OK", "xhr", "loadDataWithXMLHttpRequest (XHR)", "4.2 kB", "38 ms", "120" },
                new string[] { "GetLopOptions", "200 OK", "fetch", "loadLopDropdowns (Fetch API)", "1.8 kB", "24 ms", "80" },
                new string[] { "Search?keyword=Nguyen", "200 OK", "fetch", "searchWithFetch (Fetch API)", "3.1 kB", "31 ms", "95" },
                new string[] { "Create", "200 OK", "xhr", "$.ajax (jQuery POST)", "1.2 kB", "45 ms", "140" },
                new string[] { "GetPagedData?page=2&pageSize=10", "200 OK", "xhr", "renderPaginationDOM (Skip/Take)", "4.3 kB", "35 ms", "110" },
                new string[] { "Delete", "200 OK", "xhr", "$.ajax (jQuery DELETE/POST)", "680 B", "28 ms", "85" }
            };

            using var textCellFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            using var statusOkBrush = new SolidBrush(Color.FromArgb(129, 201, 149)); // Green 200 OK
            using var nameBrush = new SolidBrush(Color.FromArgb(144, 202, 249));

            int rowY = 96;
            for (int i = 0; i < requests.Length; i++)
            {
                var req = requests[i];
                using var rowBg = new SolidBrush(i % 2 == 0 ? Color.FromArgb(32, 32, 32) : Color.FromArgb(38, 38, 38));
                g.FillRectangle(rowBg, 0, rowY, width, 32);

                g.DrawString(req[0], textCellFont, nameBrush, 20, rowY + 8);
                g.DrawString(req[1], textCellFont, statusOkBrush, 360, rowY + 8);
                g.DrawString(req[2], textCellFont, tabTextBrush, 430, rowY + 8);
                g.DrawString(req[3], textCellFont, tabTextBrush, 520, rowY + 8);
                g.DrawString(req[4], textCellFont, tabTextBrush, 660, rowY + 8);
                g.DrawString(req[5], textCellFont, tabTextBrush, 750, rowY + 8);

                // Waterfall bar
                int barW = int.Parse(req[6]);
                using var barBrushWaterfall = new SolidBrush(Color.FromArgb(66, 133, 244));
                g.FillRectangle(barBrushWaterfall, 850 + (i * 20), rowY + 11, barW, 10);

                rowY += 32;
            }

            // Bottom inspection details pane
            using var detailBg = new SolidBrush(Color.FromArgb(28, 28, 28));
            g.FillRectangle(detailBg, 0, 300, width, 180);
            using var detailBorder = new Pen(Color.FromArgb(60, 60, 60), 1);
            g.DrawLine(detailBorder, 0, 300, width, 300);

            using var headerFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var jsonFont = new Font("Consolas", 8.5f, FontStyle.Regular);
            using var yellowBrush = new SolidBrush(Color.FromArgb(255, 214, 102));

            g.DrawString("Headers | Preview | Response (JSON Payload Preview) | Initiator | Timing", headerFont, white, 20, 310);
            g.DrawLine(detailBorder, 20, 332, 500, 332);

            string jsonPreview =
                "{\n" +
                "  \"success\": true,\n" +
                "  \"items\": [\n" +
                "    { \"maSV\": \"SV2023001\", \"hoTen\": \"Nguyễn Văn An\", \"ngaySinhFormatted\": \"15/03/2005\", \"gioiTinhText\": \"Nam\", \"tenLop\": \"CNTT01\", \"trangThaiText\": \"Đang học\" },\n" +
                "    { \"maSV\": \"SV2023002\", \"hoTen\": \"Trần Thị Bình\", \"ngaySinhFormatted\": \"22/07/2005\", \"gioiTinhText\": \"Nữ\", \"tenLop\": \"CNTT01\", \"trangThaiText\": \"Đang học\" }\n" +
                "  ],\n" +
                "  \"pageIndex\": 1, \"pageSize\": 10, \"totalItems\": 200, \"totalPages\": 20, \"message\": \"Tải dữ liệu thành công\"\n" +
                "}";

            g.DrawString(jsonPreview, jsonFont, yellowBrush, 20, 340);

            bmp.Save(outPath, ImageFormat.Png);
            return outPath;
        }
    }
}

