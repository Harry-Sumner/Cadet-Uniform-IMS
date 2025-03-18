using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Identity;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    public class IndexModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public IndexModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public IList<Stock> Stock { get;set; } = default!;
        public IList<Uniform> Uniform { get; set; } = default!;
        public IList<UniformType> UniformTypes { get; set; } = default!;
        public IList<SizeAttribute> SizeAttributes { get; set; } = default!;
        public IList<StockSize> StockSizes { get; set; } = default!;
        public int countAttributes = 0;

        public async Task OnGetAsync()
        {
            Stock = await _context.Stock.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            UniformTypes = await _context.UniformType.ToListAsync();
            SizeAttributes = await _context.SizeAttribute.ToListAsync();
            StockSizes = await _context.StockSize.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id) //takes id passed from button
        {
            var stock = await _context.Stock.FindAsync(id); //locate stock in Stock database and save in variable

            if (stock != null)
            {
                if (stock.Quantity == 1) //If there is only 1 item then remove it from stock file and remove size
                {
                    foreach(var size in _context.StockSize.Where(i=> i.StockID == stock.StockID))
                    {
                        _context.StockSize.Remove(size);
                    }
                    await _context.SaveChangesAsync();
                    _context.Stock.Remove(stock);
                }
                else //if more than 1 then decrease the quantity by 1 and update data
                {
                    stock.Quantity -= 1;
                    _context.Stock.Update(stock); //update with new item details
                }

                await _context.SaveChangesAsync(); //save changes
            }
            await OnGetAsync();
            return Page(); //return to page
        }
    }
}
