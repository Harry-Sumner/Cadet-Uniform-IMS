using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    [Authorize(Roles = "Staff, Admin")]
    public class ReturnsModel : PageModel
    {
        private readonly IMS_Context _context;

        public ReturnsModel(IMS_Context context)
        {
            _context = context;
        }

        public List<ReturnStock> ReturnStocks { get; set; } = new();
        public List<ReturnSize> ReturnSizes { get; set; } = new();
        public List<SizeAttribute> SizeAttributes { get; set; } = new();
        public List<Uniform> Uniforms { get; set; } = new();

        [TempData]
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ReturnStocks = await _context.ReturnStock.ToListAsync();
            ReturnSizes = await _context.ReturnSize.ToListAsync();
            SizeAttributes = await _context.SizeAttribute.ToListAsync();
            Uniforms = await _context.Uniform.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int returnId)
        {
            var returnStock = await _context.ReturnStock.FindAsync(returnId);
            if (returnStock == null || returnStock.Quantity <= 0)
            {
                Message = "Return item not found or already fully processed.";
                await OnGetAsync();
                return RedirectToPage();
            }

            var sizes = await _context.ReturnSize
                .Where(rs => rs.ReturnID == returnId)
                .ToListAsync();

            var existingStocks = await _context.Stock
                .Where(s => s.UniformID == returnStock.UniformID)
                .ToListAsync();

            Stock matchingStock = null;

            foreach (var stock in existingStocks)
            {
                var stockSizes = await _context.StockSize
                    .Where(ss => ss.StockID == stock.StockID)
                    .ToListAsync();

                bool match = sizes.Count == stockSizes.Count &&
                             sizes.All(rs => stockSizes.Any(ss =>
                                ss.AttributeID == rs.AttributeID &&
                                ss.Size == rs.Size));

                if (match)
                {
                    matchingStock = stock;
                    break;
                }
            }

            if (matchingStock != null)
            {
                matchingStock.Quantity += 1;
                matchingStock.Available += 1;
                _context.Stock.Update(matchingStock);
            }
            else
            {
                var currentStock = await _context.Stock
                    .OrderByDescending(r => r.StockID)
                    .FirstOrDefaultAsync();
                var newStock = new Stock
                {
                    StockID = currentStock == null ? 1 : currentStock.StockID + 1,
                    UniformID = returnStock.UniformID,
                    Quantity = 1,
                    Available = 1
                };

                _context.Stock.Add(newStock);
                await _context.SaveChangesAsync();

                foreach (var rs in sizes)
                {
                    _context.StockSize.Add(new StockSize
                    {
                        StockID = newStock.StockID,
                        AttributeID = rs.AttributeID,
                        Size = rs.Size
                    });
                }
            }

            returnStock.Quantity -= 1;

            if (returnStock.Quantity <= 0)
            {
                _context.ReturnStock.Remove(returnStock);
                _context.ReturnSize.RemoveRange(sizes);
            }
            else
            {
                _context.ReturnStock.Update(returnStock);
            }

            await _context.SaveChangesAsync();

            Message = "Return item accepted and moved to stock.";

            await OnGetAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int returnId)
        {
            var returnStock = await _context.ReturnStock.FindAsync(returnId);
            if (returnStock == null)
            {
                Message = "Return stock not found.";
                return RedirectToPage();
            }

            if (returnStock.Quantity > 1)
            {
                returnStock.Quantity -= 1;
                await _context.SaveChangesAsync();
                Message = "Return stock quantity reduced by 1.";
            }
            else
            {
                var sizesToRemove = _context.ReturnSize.Where(rs => rs.ReturnID == returnId);
                _context.ReturnSize.RemoveRange(sizesToRemove);
                _context.ReturnStock.Remove(returnStock);
                await _context.SaveChangesAsync();
                Message = "Return stock fully removed.";
            }

            return RedirectToPage();
        }
    }
}