using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;

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

        public async Task OnGetAsync()
        {
            Stock = await _context.Stock.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
        }
    }
}
