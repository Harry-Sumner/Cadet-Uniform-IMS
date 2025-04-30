using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;

public class ViewMyOrdersModel : PageModel
{
    private readonly UserManager<IMS_User> _userManager;
    private readonly IMS_Context _context;

    public ViewMyOrdersModel(UserManager<IMS_User> userManager, IMS_Context context)
    {
        _userManager = userManager;
        _context = context;
    }

    public List<OrderHistory> UserOrders { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<PendingOrder> PendingOrders { get; set; } = new();
    public List<PendingOrderItem> PendingOrderItems { get; set; } = new();
    public List<Uniform> Uniform { get; set; } = new();
    public List<Stock> Stock { get; set; } = new();
    public List<StockSize> StockSizes { get; set; } = new();
    public List<SizeAttribute> SizeAttributes { get; set; } = new();

    [TempData]
    public string Message { get; set; }

    public async Task OnGetAsync()
    {
        StockSizes = await _context.StockSize.ToListAsync();
        SizeAttributes = await _context.SizeAttribute.ToListAsync();

        var user = await _userManager.GetUserAsync(User);

        if (User.IsInRole("Cadet"))
        {
            UserOrders = await _context.OrderHistory
                .Where(o => o.Cadet == user.Id)
                .ToListAsync();

            PendingOrders = await _context.PendingOrder
                .Where(p => p.UID == user.Id)
                .ToListAsync();

            PendingOrderItems = await _context.PendingOrderItems.ToListAsync();
        }
        else
        {
            UserOrders = await _context.OrderHistory
                .Where(o => o.Cadet == null && o.UID == user.Id)
                .ToListAsync();
        }

        OrderItems = await _context.OrderItems.ToListAsync();
        Uniform = await _context.Uniform.ToListAsync();
        Stock = await _context.Stock.ToListAsync();
    }
}