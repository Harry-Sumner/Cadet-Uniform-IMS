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
        public OrderHistory Order = new();
        public PendingOrder PendingOrder = new();

        [TempData]
        public string Message { get; set; }

        public int countAttributes = 0;

        [BindProperty]
        public string SelectedCadetId { get; set; }
        public List<IMS_User> Cadets { get; set; } = new();

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
            var users = await _UserManager.Users.ToListAsync();
            Cadets = (await _UserManager.GetUsersInRoleAsync("Cadet"))
            .OrderBy(c => c.Surname)
            .ToList();


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
        public async Task<IActionResult> OnPostSubmitStaffAsync()
        {
            var currentOrder = _context.OrderHistory.FromSqlRaw("SELECT * FROM OrderHistory")
                .OrderByDescending(b => b.OrderID)
                .FirstOrDefault();

            if (currentOrder == null)
            {
                Order.OrderID = 1;
            }
            else
            {
                Order.OrderID = currentOrder.OrderID + 1;
            }

            var user = await _UserManager.GetUserAsync(User);
            Order.UID = user.Id;

            if (!string.IsNullOrEmpty(SelectedCadetId))
            {
                Order.Cadet = SelectedCadetId;
            }

            _context.OrderHistory.Add(Order); 

            var basketStock =
                _context.BasketStock
                .FromSqlRaw("SELECT * FROM BasketStock WHERE UID = {0}", user.Id)
                .ToList();

            foreach (var item in basketStock)
            {
                var stockItem = await _context.Stock.FirstOrDefaultAsync(s => s.StockID == item.StockID);
                if (stockItem != null)
                {
                    stockItem.Quantity -= item.Quantity;
                    _context.Stock.Update(stockItem);
                }
                OrderItem oi = new OrderItem
                {
                    OrderID = Order.OrderID,
                    StockID = item.StockID,
                    Quantity = item.Quantity
                };
                _context.OrderItems.Add(oi);
                _context.BasketStock.Remove(item);
            }
            await _context.SaveChangesAsync();
            Message = "The uniform selected is now ready for collection and stock has been adjusted successfully.";
            await OnGetAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostSubmitCadetAsync()
        {
            var currentPendingOrder = _context.PendingOrder.FromSqlRaw("SELECT * FROM PendingOrder")
                .OrderByDescending(b => b.PendingOrderID)
                .FirstOrDefault();

            if (currentPendingOrder == null)
            {
                PendingOrder.PendingOrderID = 1;
            }
            else
            {
                PendingOrder.PendingOrderID = currentPendingOrder.PendingOrderID + 1;
            }

            var user = await _UserManager.GetUserAsync(User);
            PendingOrder.UID = user.Id;
            _context.PendingOrder.Add(PendingOrder);

            var basketStock =
                _context.BasketStock
                .FromSqlRaw("SELECT * FROM BasketStock WHERE UID = {0}", user.Id)
                .ToList();

            foreach (var item in basketStock)
            {
                PendingOrderItem poi = new PendingOrderItem
                {
                    PendingOrderID = PendingOrder.PendingOrderID,
                    StockID = item.StockID,
                    Quantity = item.Quantity
                };
                _context.PendingOrderItems.Add(poi);
                _context.BasketStock.Remove(item);
            }
            await _context.SaveChangesAsync();
            Message = "Your uniform request has been placed and staff have been notified. Please visit uniform stores on your next parade night and speak to staff to collect your order.";
            await OnGetAsync();
            return Page();
        }
    }
}
