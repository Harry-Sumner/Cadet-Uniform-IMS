using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    [Authorize(Roles ="Admin")]
    public class BrowseUniformModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public BrowseUniformModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public IList<Uniform> Uniform { get;set; } = default!;

        public IList<UniformType> Types { get; set; } = default!;

        public IList<SizeAttribute> Attributes { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            Types = await _context.UniformType.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            Attributes = await _context.SizeAttribute.ToListAsync();
            return Page();

        }

        public async Task<IActionResult> OnDeleteType(int id)
        {
            var uniforms = await _context.Uniform.Where(i => i.TypeID == id).ToListAsync();
            if (uniforms.Any())
            {
                foreach (var uniform in uniforms)
                {
                    var stocks = await _context.Stock.Where(i => i.UniformID == uniform.UniformID ).ToListAsync();
                    if (stocks.Any())
                    {
                        foreach (var stock in stocks) {
                            var StockSize = await _context.StockSize.Where(i => i.StockID == stock.StockID).ToListAsync();
                            if (!StockSize.Any())
                            {
                                foreach (var size in StockSize)
                                {
                                    _context.StockSize.Remove(size);
                                }
                            }
                            _context.Stock.Remove(stock);
                        }
                    }
                    _context.Uniform.Remove(uniform);
                }
            }

            var attributes = await _context.SizeAttribute.Where(i => i.TypeID == id).ToListAsync();
            if (attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    _context.SizeAttribute.Remove(attribute);
                }
            }

            var type = await _context.UniformType.FindAsync(id);
            _context.UniformType.Remove(type);
            await OnGet();
            return Page();
        }

    }
}
