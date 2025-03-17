using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cadet_Uniform_IMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    public class CreateModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public CreateModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            Uniform = await _context.Uniform.ToListAsync();
            Attributes = await _context.SizeAttribute.ToListAsync();

            return Page();
        }

        [BindProperty]
        public Stock Stock { get; set; } = default!;
        public IList<Uniform> Uniform { get; set; } = default!;
        public StockSize StockSize { get; set; } = default!;

        public IList<SizeAttribute> Attributes { get; set; } = default!;
        public async Task<IActionResult> OnPostAsync()
        {
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(Dictionary<int, string> Attributes)
        {
            // Find existing stock entries for the same uniform
            var existingStocks = _context.Stock
                .Where(s => s.UniformID == Stock.UniformID)
                .Select(s => new
                {
                    Stock = s,
                    StockSizes = _context.StockSize.Where(ss => ss.StockID == s.StockID).ToList()
                })
                .ToList();

            // Look for a stock entry that matches exactly in sizes
            var matchingStock = existingStocks.FirstOrDefault(s =>
                s.StockSizes.Count == Attributes.Count && // Same number of attributes
                s.StockSizes.All(ss => Attributes.ContainsKey(ss.AttributeID) && Attributes[ss.AttributeID] == ss.Size) // Exact match
            );

            if (matchingStock != null)
            {
                // If stock already exists, increase the quantity
                matchingStock.Stock.Quantity += Stock.Quantity;
                await _context.SaveChangesAsync();
            }
            else
            {
                var currentStock = _context.Stock.FromSqlRaw("SELECT * FROM Stock")
                    .OrderByDescending(b => b.StockID)
                    .FirstOrDefault();

                if (currentStock == null)
                {
                    Stock.StockID = 1;
                }
                else
                {
                    Stock.StockID = currentStock.StockID + 1;
                }
                // If no exact match, create a new Stock entry
                _context.Stock.Add(Stock);
                await _context.SaveChangesAsync(); // Generates StockID

                // Add StockSize entries for the new stock
                foreach (var entry in Attributes)
                {
                    var stockSize = new StockSize
                    {
                        StockID = Stock.StockID,  // Foreign key reference to Stock
                        AttributeID = entry.Key,  // Attribute ID
                        Size = entry.Value        // User input size
                    };
                    _context.StockSize.Add(stockSize);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
