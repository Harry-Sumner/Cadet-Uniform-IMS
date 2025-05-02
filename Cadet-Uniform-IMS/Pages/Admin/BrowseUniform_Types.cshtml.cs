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

            foreach (var uniform in uniforms)
            {
                var stocks = await _context.Stock.Where(s => s.UniformID == uniform.UniformID).ToListAsync();

                foreach (var stock in stocks)
                {
                    _context.BasketStock.RemoveRange(await _context.BasketStock.Where(b => b.StockID == stock.StockID).ToListAsync());
                    _context.PendingOrderItems.RemoveRange(await _context.PendingOrderItems.Where(p => p.StockID == stock.StockID).ToListAsync());
                    _context.OrderItems.RemoveRange(await _context.OrderItems.Where(o => o.StockID == stock.StockID).ToListAsync());
                    _context.ReturnStock.RemoveRange(await _context.ReturnStock.Where(r => r.ReturnID == stock.StockID).ToListAsync());
                    _context.ReturnSize.RemoveRange(await _context.ReturnSize.Where(rs => rs.ReturnID == stock.StockID).ToListAsync());
                    _context.StockSize.RemoveRange(await _context.StockSize.Where(s => s.StockID == stock.StockID).ToListAsync());
                }

                _context.Stock.RemoveRange(stocks);
                _context.Uniform.Remove(uniform);
            }

            var attributes = await _context.SizeAttribute.Where(a => a.TypeID == id).ToListAsync();

            foreach (var attr in attributes)
            {
                _context.StockSize.RemoveRange(await _context.StockSize.Where(s => s.AttributeID == attr.AttributeID).ToListAsync());
                _context.ReturnSize.RemoveRange(await _context.ReturnSize.Where(r => r.AttributeID == attr.AttributeID).ToListAsync());
            }

            _context.SizeAttribute.RemoveRange(attributes);
            var type = await _context.UniformType.FindAsync(id);
            if (type != null)
                _context.UniformType.Remove(type);

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUniformAsync(int id)
        {
            var uniform = await _context.Uniform.FindAsync(id);
            if (uniform == null)
                return NotFound();

            var stocks = await _context.Stock
                .Where(s => s.UniformID == uniform.UniformID)
                .ToListAsync();

            foreach (var stock in stocks)
            {
                _context.BasketStock.RemoveRange(await _context.BasketStock.Where(b => b.StockID == stock.StockID).ToListAsync());
                _context.PendingOrderItems.RemoveRange(await _context.PendingOrderItems.Where(p => p.StockID == stock.StockID).ToListAsync());
                _context.OrderItems.RemoveRange(await _context.OrderItems.Where(o => o.StockID == stock.StockID).ToListAsync());
                _context.ReturnStock.RemoveRange(await _context.ReturnStock.Where(r => r.ReturnID == stock.StockID).ToListAsync());
                _context.ReturnSize.RemoveRange(await _context.ReturnSize.Where(rs => rs.ReturnID == stock.StockID).ToListAsync());
                _context.StockSize.RemoveRange(await _context.StockSize.Where(s => s.StockID == stock.StockID).ToListAsync());
            }

            _context.Stock.RemoveRange(stocks);
            _context.Uniform.Remove(uniform);

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteStockAsync(int id)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock == null)
                return NotFound();

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
                    _context.BasketStock.Update(item);
                    quantityToRemove = 0;
                }
            }

            var pendingItems = await _context.PendingOrderItems
                .Where(p => p.StockID == id)
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
                    _context.PendingOrderItems.Update(item);
                    quantityToRemove = 0;
                }
            }

            stock.Quantity -= 1;
            stock.Available -= 1;

            if (stock.Quantity <= 0)
            {
                var stockSizes = await _context.StockSize.Where(ss => ss.StockID == id).ToListAsync();
                _context.StockSize.RemoveRange(stockSizes);

                var orderItems = await _context.OrderItems.Where(o => o.StockID == id).ToListAsync();
                _context.OrderItems.RemoveRange(orderItems);

                _context.Stock.Remove(stock);
            }
            else
            {
                _context.Stock.Update(stock);
            }

            await _context.SaveChangesAsync();
            await OnGet();
            return Page();
        }
    }
}
