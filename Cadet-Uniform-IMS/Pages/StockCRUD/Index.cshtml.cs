using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Cadet_Uniform_IMS.Pages.StockCRUD
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMS_Context _context;
        private readonly UserManager<IMS_User> _userManager;

        public IndexModel(IMS_Context context, UserManager<IMS_User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Stock> Stock { get; set; } = new List<Stock>();
        public IList<Uniform> Uniform { get; set; } = new List<Uniform>();
        public IList<UniformType> UniformTypes { get; set; } = new List<UniformType>();
        public IList<SizeAttribute> SizeAttributes { get; set; } = new List<SizeAttribute>();
        public IList<StockSize> StockSizes { get; set; } = new List<StockSize>();

        public int countAttributes = 0;

        [BindProperty(SupportsGet = true)]
        public string? SelectedSize { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool FilterByUserMeasurements { get; set; } = false;

        public async Task OnGetAsync()
        {
            Stock = await _context.Stock.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            UniformTypes = await _context.UniformType.ToListAsync();
            SizeAttributes = await _context.SizeAttribute.ToListAsync();
            StockSizes = await _context.StockSize.ToListAsync();

            if (!string.IsNullOrWhiteSpace(SelectedSize))
            {
                var stockIdsWithSize = StockSizes
                    .Where(ss => ss.Size.Equals(SelectedSize, StringComparison.OrdinalIgnoreCase))
                    .Select(ss => ss.StockID)
                    .ToHashSet();

                Stock = Stock.Where(s => stockIdsWithSize.Contains(s.StockID)).ToList();
            }

            if (FilterByUserMeasurements)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var attributeToUserMeasurement = new Dictionary<string, int?>
                    {
                        { "Collar", currentUser.Neck },
                        { "Length", currentUser.Height },
                        { "Chest", currentUser.Chest },
                        { "Waist", currentUser.Waist },
                        { "Waist Knee", currentUser.WaistKnee },
                        { "Leg", currentUser.Leg },
                        { "Hips", currentUser.Hips },
                        { "Seat", currentUser.Seat },
                        { "Head", currentUser.Head },
                        { "Shoe Size", currentUser.Shoe }
                    };

                    var filteredStock = new List<Stock>();

                    foreach (var stockItem in Stock)
                    {
                        var uniform = Uniform.FirstOrDefault(u => u.UniformID == stockItem.UniformID);
                        if (uniform == null) continue;

                        var attributes = SizeAttributes.Where(sa => sa.TypeID == uniform.TypeID).ToList();
                        bool matchesAll = true;

                        foreach (var attr in attributes)
                        {
                            if (!attributeToUserMeasurement.TryGetValue(attr.AttributeName, out int? userMeasurement) || userMeasurement == null)
                            {
                                matchesAll = false;
                                break;
                            }

                            var stockSizes = StockSizes
                                .Where(ss => ss.StockID == stockItem.StockID && ss.AttributeID == attr.AttributeID)
                                .Select(ss => ss.Size)
                                .ToList();

                            bool sizeMatch = false;
                            foreach (var sizeStr in stockSizes)
                            {
                                if (int.TryParse(sizeStr, out int sizeVal))
                                {
                                    int threshold = attr.AttributeName.Equals("Shoe Size", StringComparison.OrdinalIgnoreCase) ? 1 : 3;
                                    if (Math.Abs(sizeVal - userMeasurement.Value) <= threshold)
                                    {
                                        sizeMatch = true;
                                        break;
                                    }
                                }
                            }

                            if (!sizeMatch)
                            {
                                matchesAll = false;
                                break;
                            }
                        }

                        if (matchesAll)
                        {
                            filteredStock.Add(stockItem);
                        }
                    }
                    Stock = filteredStock;
                }
            }
        }

        public async Task<IActionResult> OnPostAddAsync(int stockID)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var stockItem = await _context.Stock.FirstOrDefaultAsync(s => s.StockID == stockID);

                if (stockItem == null || stockItem.Available <= 0)
                {
                    return NotFound(); 
                }

                var item = await _context.BasketStock
                    .FirstOrDefaultAsync(bs => bs.StockID == stockID && bs.UID == user.Id);

                if (item == null)
                {
                    BasketStock newStock = new BasketStock
                    {
                        StockID = stockID,
                        UID = user.Id,
                        Quantity = 1
                    };
                    _context.BasketStock.Add(newStock);
                }
                else
                {
                    item.Quantity += 1;
                    _context.Attach(item).State = EntityState.Modified;
                }
                stockItem.Available -= 1;
                _context.Stock.Update(stockItem);

                await _context.SaveChangesAsync();
            }
            await OnGetAsync();
            return Page();
        }
    }
}