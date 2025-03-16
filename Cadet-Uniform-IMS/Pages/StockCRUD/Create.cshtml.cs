using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cadet_Uniform_IMS.Data;
using Microsoft.EntityFrameworkCore;

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
            return Page();
        }

        [BindProperty]
        public Stock Stock { get; set; } = default!;
        public IList<Uniform> Uniform { get; set; } = default!;
        public StockSize StockSize { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }

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

          
            _context.Stock.Add(Stock);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
