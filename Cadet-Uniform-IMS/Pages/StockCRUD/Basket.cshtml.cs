using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    public class BasketModel : PageModel
    {
        private readonly IMS_Context _context;
        private readonly UserManager<IMS_User> _UserManager;
        public IList<BasketStock> Items { get; private set; }
        public IList<Stock> Stock { get; set; } = new List<Stock>();
        public IList<Uniform> Uniform { get; set; } = new List<Uniform>();
        public IList<SizeAttribute> SizeAttributes { get; set; } = new List<SizeAttribute>();
        public IList<StockSize> StockSizes { get; set; } = new List<StockSize>();

        public int countAttributes = 0;

        public BasketModel(IMS_Context context, UserManager<IMS_User> userManager)
        {
            _context = context;
            _UserManager = userManager;

        }
        public async Task OnGetAsync()
        {
            var user = await _UserManager.GetUserAsync(User);

            Items = _context.BasketStock.Where(bs => bs.UID  == user.Id).ToList();
            Stock = await _context.Stock.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            SizeAttributes = await _context.SizeAttribute.ToListAsync();
            StockSizes = await _context.StockSize.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int stockID)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user != null)
            {
                var item = await _context.BasketStock
                    .FirstOrDefaultAsync(bs => bs.StockID == stockID && bs.UID == user.Id);

                if (item != null)
                {
                    var stockItem = await _context.Stock.FirstOrDefaultAsync(s => s.StockID == stockID);

                    if (item.Quantity > 1)
                    {
                        item.Quantity -= 1;
                        _context.BasketStock.Update(item);
                    }
                    else
                    {
                        _context.BasketStock.Remove(item);
                    }

                    if (stockItem != null)
                    {
                        stockItem.Available += 1;
                        _context.Stock.Update(stockItem);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            await OnGetAsync();
            return Page();
        }
    }
}
