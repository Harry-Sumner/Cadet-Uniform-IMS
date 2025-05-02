using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;
using SelectPdf;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Cadet_Uniform_IMS.Pages
{
    public class ReportModel : PageModel
    {
        private readonly IMS_Context _context;

        public ReportModel(IMS_Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Category { get; set; }

        [BindProperty]
        public string ReportType { get; set; }

        public List<UniformType> UniformTypes { get; set; } = new();
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            UniformTypes = await _context.UniformType.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            UniformTypes = await _context.UniformType.ToListAsync();

            // Check if a report type was selected
            if (string.IsNullOrEmpty(ReportType))
            {
                StatusMessage = "Please select a report type.";
                return Page();
            }

            try
            {
                if (ReportType == "StockReport")
                    return await GenerateStockReport();
                else if (ReportType == "PendingOrderReport")
                    return await GeneratePendingOrderReport();
                else if (ReportType == "ReturnStockReport")
                    return await GenerateReturnStockReport();

                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating the report: {ex.Message}";
                return Page();
            }
        }
        // Code adapted from (SelectPDF Free HTML to PDF Converter for .NET C# / ASP.NET - PDF Library for .NET Sample Code, n.d.)
        private async Task<IActionResult> GenerateStockReport()
        {
            var query = @"
                SELECT 
                    u.Name AS UniformName,
                    ut.TypeName AS UniformType,
                    s.Quantity,
                    s.Available,
                    sa.AttributeName,
                    ss.Size
                FROM Stock s
                INNER JOIN Uniform u ON u.UniformID = s.UniformID
                INNER JOIN UniformType ut ON ut.TypeID = u.TypeID
                INNER JOIN StockSize ss ON ss.StockID = s.StockID
                INNER JOIN SizeAttribute sa ON sa.AttributeID = ss.AttributeID
                WHERE (@category IS NULL OR @category = '' OR ut.TypeName = @category)
                ORDER BY u.Name, sa.AttributeName, ss.Size";

            var parameters = new[] {
                new SqlParameter("@category", Category ?? (object)DBNull.Value)
            };

            var data = await _context.StockReportItem.FromSqlRaw(query, parameters).ToListAsync();

            if (data == null || data.Count == 0)
            {
                StatusMessage = "No data found for the selected category.";
                return Page();
            }

            var grouped = data
                .GroupBy(x => new { x.UniformName, x.UniformType, x.Quantity, x.Available })
                .Select(g => new
                {
                    g.Key.UniformName,
                    g.Key.UniformType,
                    g.Key.Quantity,
                    g.Key.Available,
                    Sizes = g.GroupBy(x => x.AttributeName)
                             .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(z => z.Size)))
                })
                .ToList();

            var html = new StringBuilder();
            html.Append("<html><head><style>");
            html.Append("table { width: 100%; border-collapse: collapse; font-family: Arial; font-size: 10pt; }");
            html.Append("th, td { border: 1px solid #000; padding: 5px; text-align: left; }");
            html.Append("th { background-color: #f2f2f2; }");
            html.Append("</style></head><body>");

            html.Append($"<h2>Uniform Stock Report - {(string.IsNullOrEmpty(Category) ? "All" : Category)}</h2>");
            html.Append($"<p>Generated: {DateTime.Now:dd/MM/yyyy}</p>");
            html.Append("<table>");
            html.Append("<tr><th>Uniform Name</th><th>Category</th><th>Sizes</th><th>Quantity</th><th>Available</th></tr>");

            foreach (var item in grouped)
            {
                var sizeText = string.Join("; ", item.Sizes.Select(s => $"{s.Key}: {s.Value}"));
                html.Append("<tr>");
                html.Append($"<td>{item.UniformName}</td>");
                html.Append($"<td>{item.UniformType}</td>");
                html.Append($"<td>{sizeText}</td>");
                html.Append($"<td>{item.Quantity}</td>");
                html.Append($"<td>{item.Available}</td>");
                html.Append("</tr>");
            }

            html.Append("</table></body></html>");

            var converter = new HtmlToPdf();
            var pdf = converter.ConvertHtmlString(html.ToString());

            var pdfBytes = pdf.Save();
            pdf.Close();

            return File(pdfBytes, "application/pdf", $"StockReport_{Category ?? "All"}.pdf");
        }

        private async Task<IActionResult> GeneratePendingOrderReport()
        {
            var query = @"
                SELECT 
                    u.Name AS UniformName,
                    ut.TypeName AS UniformType,
                    poi.Quantity,
                    sa.AttributeName,
                    ss.Size,
                    asp.UserName
                FROM PendingOrderItems poi
                INNER JOIN PendingOrder po ON poi.PendingOrderID = po.PendingOrderID
                INNER JOIN Stock s ON poi.StockID = s.StockID
                INNER JOIN Uniform u ON s.UniformID = u.UniformID
                INNER JOIN UniformType ut ON u.TypeID = ut.TypeID
                INNER JOIN StockSize ss ON ss.StockID = s.StockID
                INNER JOIN SizeAttribute sa ON sa.AttributeID = ss.AttributeID
                INNER JOIN AspNetUsers asp ON po.UID = asp.Id
                WHERE (@category IS NULL OR @category = '' OR ut.TypeName = @category)
                ORDER BY asp.UserName, u.Name, sa.AttributeName, ss.Size";

            var parameters = new[] {
                new SqlParameter("@category", Category ?? (object)DBNull.Value)
            };

            var data = await _context.PendingReportItem.FromSqlRaw(query, parameters).ToListAsync();

            if (data == null || data.Count == 0)
            {
                StatusMessage = "No pending order data found for the selected category.";
                return Page();
            }

            var grouped = data
                .GroupBy(x => new { x.Username, x.UniformName, x.UniformType, x.Quantity })
                .Select(g => new
                {
                    g.Key.Username,
                    g.Key.UniformName,
                    g.Key.UniformType,
                    g.Key.Quantity,
                    Sizes = g.GroupBy(x => x.AttributeName)
                             .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(z => z.Size)))
                })
                .ToList();

            var html = new StringBuilder();
            html.Append("<html><head><style>");
            html.Append("table { width: 100%; border-collapse: collapse; font-family: Arial; font-size: 10pt; }");
            html.Append("th, td { border: 1px solid #000; padding: 5px; text-align: left; }");
            html.Append("th { background-color: #f2f2f2; }");
            html.Append("</style></head><body>");

            html.Append($"<h2>Pending Orders Report - {(string.IsNullOrEmpty(Category) ? "All" : Category)}</h2>");
            html.Append($"<p>Generated: {DateTime.Now:dd/MM/yyyy}</p>");
            html.Append("<table>");
            html.Append("<tr><th>User</th><th>Uniform Name</th><th>Category</th><th>Sizes</th><th>Quantity</th></tr>");

            foreach (var item in grouped)
            {
                var sizeText = string.Join("; ", item.Sizes.Select(s => $"{s.Key}: {s.Value}"));
                html.Append("<tr>");
                html.Append($"<td>{item.Username}</td>");
                html.Append($"<td>{item.UniformName}</td>");
                html.Append($"<td>{item.UniformType}</td>");
                html.Append($"<td>{sizeText}</td>");
                html.Append($"<td>{item.Quantity}</td>");
                html.Append("</tr>");
            }

            html.Append("</table></body></html>");

            var converter = new HtmlToPdf();
            var pdf = converter.ConvertHtmlString(html.ToString());

            var pdfBytes = pdf.Save();
            pdf.Close();

            return File(pdfBytes, "application/pdf", $"PendingOrders_{Category ?? "All"}.pdf");
        }

        private async Task<IActionResult> GenerateReturnStockReport()
        {
            var query = @"
        SELECT 
            u.Name AS UniformName,
            ut.TypeName AS UniformType,
            rs.Quantity,
            sa.AttributeName,
            rsz.Size
        FROM ReturnStock rs
        INNER JOIN Uniform u ON u.UniformID = rs.UniformID
        INNER JOIN UniformType ut ON ut.TypeID = u.TypeID
        INNER JOIN ReturnSize rsz ON rsz.ReturnID = rs.ReturnID
        INNER JOIN SizeAttribute sa ON sa.AttributeID = rsz.AttributeID
        WHERE (@category IS NULL OR @category = '' OR ut.TypeName = @category)
        ORDER BY u.Name, sa.AttributeName, rsz.Size";

            var parameters = new[] {
            new SqlParameter("@category", Category ?? (object)DBNull.Value)
    };

            var data = await _context.ReturnReportItem.FromSqlRaw(query, parameters).ToListAsync();

            if (data == null || data.Count == 0)
            {
                StatusMessage = "No return stock data found for the selected category.";
                return Page();
            }

            var grouped = data
                .GroupBy(x => new { x.UniformName, x.UniformType, x.Quantity })
                .Select(g => new
                {
                    g.Key.UniformName,
                    g.Key.UniformType,
                    g.Key.Quantity,
                    Sizes = g.GroupBy(x => x.AttributeName)
                             .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(z => z.Size)))
                })
                .ToList();

            var html = new StringBuilder();
            html.Append("<html><head><style>");
            html.Append("table { width: 100%; border-collapse: collapse; font-family: Arial; font-size: 10pt; }");
            html.Append("th, td { border: 1px solid #000; padding: 5px; text-align: left; }");
            html.Append("th { background-color: #f2f2f2; }");
            html.Append("</style></head><body>");

            html.Append($"<h2>Return Stock Report - {(string.IsNullOrEmpty(Category) ? "All" : Category)}</h2>");
            html.Append($"<p>Generated: {DateTime.Now:dd/MM/yyyy}</p>");
            html.Append("<table>");
            html.Append("<tr><th>Uniform Name</th><th>Category</th><th>Sizes</th><th>Quantity</th></tr>");

            foreach (var item in grouped)
            {
                var sizeText = string.Join("; ", item.Sizes.Select(s => $"{s.Key}: {s.Value}"));
                html.Append("<tr>");
                html.Append($"<td>{item.UniformName}</td>");
                html.Append($"<td>{item.UniformType}</td>");
                html.Append($"<td>{sizeText}</td>");
                html.Append($"<td>{item.Quantity}</td>");
                html.Append("</tr>");
            }

            html.Append("</table></body></html>");

            var converter = new HtmlToPdf();
            var pdf = converter.ConvertHtmlString(html.ToString());

            var pdfBytes = pdf.Save();
            pdf.Close();

            return File(pdfBytes, "application/pdf", $"ReturnStockReport_{Category ?? "All"}.pdf");
        }
        // End of code adapted
    }
}