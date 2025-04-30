using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Cadet_Uniform_IMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    [Authorize(Roles = "Staff, Admin")]
    public class CreateModel : PageModel
    {
        private readonly IMS_Context _context;

        public CreateModel(IMS_Context context)
        {
            _context = context;
        }

        public IList<Uniform> Uniform { get; set; }
        public IList<SizeAttribute> Attributes { get; set; }

        [BindProperty]
        public Stock Stock { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Uniform = await _context.Uniform.ToListAsync();
            Attributes = await _context.SizeAttribute.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(Dictionary<int, string> Attributes, string switchValue)
        {
            if (switchValue == "return")
            {
                var existingReturns = _context.ReturnStock
                    .Where(r => r.UniformID == Stock.UniformID)
                    .Select(r => new
                    {
                        Return = r,
                        Sizes = _context.ReturnSize.Where(rs => rs.ReturnID == r.ReturnID).ToList()
                    })
                    .ToList();

                var matchingReturn = existingReturns.FirstOrDefault(r =>
                    r.Sizes.Count == Attributes.Count &&
                    r.Sizes.All(rs => Attributes.ContainsKey(rs.AttributeID) && Attributes[rs.AttributeID] == rs.Size)
                );

                if (matchingReturn != null)
                {
                    matchingReturn.Return.Quantity += Stock.Quantity;
                    await _context.SaveChangesAsync();
                    Message = "Return stock updated successfully.";
                }
                else
                {
                    var currentReturn = await _context.ReturnStock
                              .OrderByDescending(r => r.ReturnID)
                              .FirstOrDefaultAsync();

                    var newReturn = new ReturnStock
                    {
                        ReturnID = currentReturn == null ? 1 : currentReturn.ReturnID + 1,
                        UniformID = Stock.UniformID,
                        Quantity = Stock.Quantity
                    };

                    _context.ReturnStock.Add(newReturn);
                    await _context.SaveChangesAsync();

                    foreach (var entry in Attributes)
                    {
                        var returnSize = new ReturnSize
                        {
                            ReturnID = newReturn.ReturnID,
                            AttributeID = entry.Key,
                            Size = entry.Value
                        };
                        _context.ReturnSize.Add(returnSize);
                    }

                    await _context.SaveChangesAsync();
                    Message = "New return stock added successfully.";
                }
            }
            else
            {
                var existingStocks = _context.Stock
                    .Where(s => s.UniformID == Stock.UniformID)
                    .Select(s => new
                    {
                        Stock = s,
                        Sizes = _context.StockSize.Where(ss => ss.StockID == s.StockID).ToList()
                    })
                    .ToList();

                var matchingStock = existingStocks.FirstOrDefault(s =>
                    s.Sizes.Count == Attributes.Count &&
                    s.Sizes.All(ss => Attributes.ContainsKey(ss.AttributeID) && Attributes[ss.AttributeID] == ss.Size)
                );

                if (matchingStock != null)
                {
                    matchingStock.Stock.Quantity += Stock.Quantity;
                    matchingStock.Stock.Available += Stock.Quantity;
                    await _context.SaveChangesAsync();
                    Message = "Existing stock updated successfully.";
                }
                else
                {
                    var currentStock = await _context.Stock
                        .OrderByDescending(s => s.StockID)
                        .FirstOrDefaultAsync();

                    Stock.StockID = currentStock == null ? 1 : currentStock.StockID + 1;
                    Stock.Available = Stock.Quantity;
                    _context.Stock.Add(Stock);
                    await _context.SaveChangesAsync();

                    foreach (var entry in Attributes)
                    {
                        var stockSize = new StockSize
                        {
                            StockID = Stock.StockID,
                            AttributeID = entry.Key,
                            Size = entry.Value
                        };
                        _context.StockSize.Add(stockSize);
                    }

                    await _context.SaveChangesAsync();
                    Message = "New stock added successfully.";
                }
            }

            return RedirectToPage();
        }
    }
}