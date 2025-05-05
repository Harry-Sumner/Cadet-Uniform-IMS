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
        public IList<Stock> Stock { get; set; } = new List<Stock>();
        public IList<UniformType> UniformTypes { get; set; } = new List<UniformType>();
        public IList<SizeAttribute> SizeAttributes { get; set; } = new List<SizeAttribute>();
        public IList<StockSize> StockSizes { get; set; } = new List<StockSize>();

        public int countAttributes = 0;

        public async Task<IActionResult> OnGet()
        {
            Types = await _context.UniformType.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            Attributes = await _context.SizeAttribute.ToListAsync();
            Stock = await _context.Stock.ToListAsync();
            SizeAttributes = await _context.SizeAttribute.ToListAsync();
            StockSizes = await _context.StockSize.ToListAsync();
            return Page();

        }

        public async Task<IActionResult> OnPostDeleteTypeAsync(int id)
        {
            var uniforms = await _context.Uniform.Where(u => u.TypeID == id).ToListAsync();

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
                            var orderItems = await _context.OrderItems.Where(os => os.StockID == stock.StockID).ToListAsync();
                            _context.OrderItems.RemoveRange(orderItems);

                            var basketItems = await _context.BasketStock.Where(bs => bs.StockID == stock.StockID).ToListAsync();
                            _context.BasketStock.RemoveRange(basketItems);

                            var pendingItems = await _context.PendingOrderItems.Where(p => p.StockID == stock.StockID).ToListAsync();
                            _context.PendingOrderItems.RemoveRange(pendingItems);

                            var stockSizes = await _context.StockSize.Where(ss => ss.StockID == stock.StockID).ToListAsync();
                            _context.StockSize.RemoveRange(stockSizes);

                            await _context.SaveChangesAsync();
                        }

                        _context.Stock.RemoveRange(stocks);
                    }

                    var returns = await _context.ReturnStock.Where(r => r.UniformID == uniform.UniformID).ToListAsync();
                    foreach (var ret in returns)
                    {
                        var returnSizes = await _context.ReturnSize.Where(rs => rs.ReturnID == ret.ReturnID).ToListAsync();
                        _context.ReturnSize.RemoveRange(returnSizes);
                    }
                    _context.ReturnStock.RemoveRange(returns);

                    _context.Uniform.Remove(uniform);
                }
            }

            var attributes = await _context.SizeAttribute.Where(i => i.TypeID == id).ToListAsync();

            if (attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    var stockSizes = await _context.StockSize.Where(ss => ss.AttributeID == attribute.AttributeID).ToListAsync();
                    _context.StockSize.RemoveRange(stockSizes);

                    var returnSizes = await _context.ReturnSize.Where(rs => rs.AttributeID == attribute.AttributeID).ToListAsync();
                    _context.ReturnSize.RemoveRange(returnSizes);
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
            var uniforms = await _context.Uniform.Where(u => u.UniformID == id).ToListAsync();

            if (uniforms.Any())
            {
                foreach (var uniform in uniforms)
                {
                    var stocks = await _context.Stock.Where(s => s.UniformID == uniform.UniformID).ToListAsync();

                    if (stocks.Any())
                    {
                        foreach (var stock in stocks)
                        {
                            var orderItems = await _context.OrderItems.Where(os => os.StockID == stock.StockID).ToListAsync();
                            _context.OrderItems.RemoveRange(orderItems);

                            var basketItems = await _context.BasketStock.Where(bs => bs.StockID == stock.StockID).ToListAsync();
                            _context.BasketStock.RemoveRange(basketItems);

                            var pendingItems = await _context.PendingOrderItems.Where(p => p.StockID == stock.StockID).ToListAsync();
                            _context.PendingOrderItems.RemoveRange(pendingItems);

                            var stockSizes = await _context.StockSize.Where(ss => ss.StockID == stock.StockID).ToListAsync();
                            _context.StockSize.RemoveRange(stockSizes);

                            await _context.SaveChangesAsync();
                        }

                        _context.Stock.RemoveRange(stocks);
                    }

                    var returns = await _context.ReturnStock.Where(r => r.UniformID == uniform.UniformID).ToListAsync();
                    foreach (var ret in returns)
                    {
                        var returnSizes = await _context.ReturnSize.Where(rs => rs.ReturnID == ret.ReturnID).ToListAsync();
                        _context.ReturnSize.RemoveRange(returnSizes);
                    }
                    _context.ReturnStock.RemoveRange(returns);

                    _context.Uniform.Remove(uniform);
                }
            }

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteStockAsync(int id)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            int quantityToRemove = 1;

            var basketItems = await _context.BasketStock
                .Where(b => b.StockID == id)
                .OrderByDescending(b => b.Quantity)
                .ToListAsync();

            foreach (var item in basketItems)
            {
                if (quantityToRemove <= 0) break;

                if (item.Quantity <= quantityToRemove)
                {
                    quantityToRemove -= item.Quantity;
                    _context.BasketStock.Remove(item);
                }
                else
                {
                    item.Quantity -= quantityToRemove;
                    quantityToRemove = 0;
                    _context.BasketStock.Update(item);
                }
            }

            if (quantityToRemove > 0)
            {
                var pendingItems = await _context.PendingOrderItems
                    .Where(p => p.StockID == id)
                    .OrderByDescending(p => p.Quantity)
                    .ToListAsync();

                foreach (var item in pendingItems)
                {
                    if (quantityToRemove <= 0) break;

                    if (item.Quantity <= quantityToRemove)
                    {
                        quantityToRemove -= item.Quantity;
                        _context.PendingOrderItems.Remove(item);
                    }
                    else
                    {
                        item.Quantity -= quantityToRemove;
                        quantityToRemove = 0;
                        _context.PendingOrderItems.Update(item);
                    }
                }
            }
            if (stock.Quantity > 0)
            {
                stock.Quantity = Math.Max(0, stock.Quantity - 1);
                stock.Available = Math.Max(0, stock.Available - 1);
                _context.Stock.Update(stock);
            }

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }
    }
}
