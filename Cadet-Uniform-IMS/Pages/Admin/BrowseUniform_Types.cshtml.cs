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
    [Authorize(Roles = "Admin")]
    public class BrowseUniformModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public BrowseUniformModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public IList<Uniform> Uniform { get; set; } = default!;

        public IList<UniformType> Types { get; set; } = default!;

        public IList<SizeAttribute> Attributes { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            Types = await _context.UniformType.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            Attributes = await _context.SizeAttribute.ToListAsync();
            return Page();

        }

        public async Task<IActionResult> OnPostDeleteTypeAsync(int id)
        {
            var uniforms = await _context.Uniform
                .Where(u => u.TypeID == id)
                .ToListAsync();

            if (uniforms.Any())
            {
                foreach (var uniform in uniforms)
                {
                    var stocks = await _context.Stock
                        .Where(s => s.UniformID == uniform.UniformID)
                        .ToListAsync();

                    if (stocks.Any())
                    {
                        foreach (var stock in stocks)
                        {
                            var stockSizes = await _context.StockSize
                                .Where(ss => ss.StockID == stock.StockID)
                                .ToListAsync();

                            _context.StockSize.RemoveRange(stockSizes);
                            await _context.SaveChangesAsync();
                        }

                        _context.Stock.RemoveRange(stocks);
                    }

                    _context.Uniform.Remove(uniform);
                }
            }

            var attributes = await _context.SizeAttribute
                .Where(i => i.TypeID == id)
                .ToListAsync();

            if (attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    var stockSizes = await _context.StockSize
                        .Where(ss => ss.AttributeID == attribute.AttributeID)
                        .ToListAsync();

                    _context.StockSize.RemoveRange(stockSizes);
                }

                _context.SizeAttribute.RemoveRange(attributes);
            }

            var type = await _context.UniformType.FindAsync(id);
            if (type != null)
            {
                _context.UniformType.Remove(type);
            }

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUniformAsync(int id)
        {
            var uniforms = await _context.Uniform
                .Where(u => u.UniformID == id)
                .ToListAsync();

            if (uniforms.Any())
            {
                foreach (var uniform in uniforms)
                {
                    var stocks = await _context.Stock
                        .Where(s => s.UniformID == uniform.UniformID)
                        .ToListAsync();

                    if (stocks.Any())
                    {
                        foreach (var stock in stocks)
                        {
                            var stockSizes = await _context.StockSize
                                .Where(ss => ss.StockID == stock.StockID)
                                .ToListAsync();

                            _context.StockSize.RemoveRange(stockSizes);
                            await _context.SaveChangesAsync();
                        }

                        _context.Stock.RemoveRange(stocks);
                    }

                    _context.Uniform.Remove(uniform);
                }
            }

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }
    }
}
