using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cadet_Uniform_IMS.Data;
using System.IO;

[Authorize(Roles = "Staff, Admin")]
public class ViewOrdersModel : PageModel
{
    private readonly UserManager<IMS_User> _userManager;
    private readonly IMS_Context _context;

    public ViewOrdersModel(UserManager<IMS_User> userManager, IMS_Context context)
    {
        _userManager = userManager;
        _context = context;
    }

    public List<IMS_Cadet> Cadets { get; set; } = new();
    public List<IMS_Staff> Staff { get; set; } = new();
    public List<PendingOrder> PendingOrders { get; set; } = new();
    public List<PendingOrderItem> PendingOrderItems { get; set; } = new();
    public List<OrderHistory> OrderHistory { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<Uniform> Uniform { get; set; } = new();
    public List<Stock> Stock { get; set; } = new();
    public List<StockSize> StockSizes { get; set; } = new();
    public List<SizeAttribute> SizeAttributes { get; set; } = new();
    public OrderHistory Order = new();

    [TempData]
    public string Message { get; set; }

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        Cadets = users.OfType<IMS_Cadet>().OrderBy(c => c.Surname).ToList();
        Staff = users.OfType<IMS_Staff>().ToList();
        PendingOrders = await _context.PendingOrder.ToListAsync();
        PendingOrderItems = await _context.PendingOrderItems.ToListAsync();
        OrderHistory = await _context.OrderHistory.ToListAsync();
        OrderItems = await _context.OrderItems.ToListAsync();
        Uniform = await _context.Uniform.ToListAsync();
        Stock = await _context.Stock.ToListAsync();
        StockSizes = await _context.StockSize.ToListAsync();
        SizeAttributes = await _context.SizeAttribute.ToListAsync();
    }

    public async Task<IActionResult> OnPostAcceptAsync(int id)
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

        var penOrder =_context.PendingOrder.FromSqlRaw("SELECT * FROM PendingOrder WHERE PendingOrderID = {0}", id).FirstOrDefault();

        Order.UID = penOrder.UID;
        Order.Cadet = penOrder.UID;

        _context.OrderHistory.Add(Order);

        var penStock =
            _context.PendingOrderItems
            .FromSqlRaw("SELECT * FROM PendingOrderItems WHERE PendingOrderID = {0}", penOrder.PendingOrderID)
            .ToList();

        foreach (var item in penStock)
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
            _context.PendingOrderItems.Remove(item);
        }
        await _context.SaveChangesAsync();
        _context.PendingOrder.Remove(penOrder);
        await _context.SaveChangesAsync();

        Message = "Pending order has been distributed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var penOrder = _context.PendingOrder.FromSqlRaw("SELECT * FROM PendingOrder WHERE PendingOrderID = {0}", id).FirstOrDefault();

        var penStock =
            _context.PendingOrderItems
            .FromSqlRaw("SELECT * FROM PendingOrderItems WHERE PendingOrderID = {0}", penOrder.PendingOrderID)
            .ToList();

        foreach (var item in penStock)
        {
            var stockItem = await _context.Stock.FirstOrDefaultAsync(s => s.StockID == item.StockID);
            if (stockItem != null)
            {
                stockItem.Available += item.Quantity;
                _context.Stock.Update(stockItem);
            }
            _context.PendingOrderItems.Remove(item);
        }
        await _context.SaveChangesAsync();
        _context.PendingOrder.Remove(penOrder);
        await _context.SaveChangesAsync();

        Message = "Pending order has been rejected.";
        return RedirectToPage();
    }
}