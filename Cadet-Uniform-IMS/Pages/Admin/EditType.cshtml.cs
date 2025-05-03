using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    public class EditTypeModel : PageModel
    {
        private readonly IMS_Context _context;

        public EditTypeModel(IMS_Context context)
        {
            _context = context;
        }

        [BindProperty]
        public UniformType Type { get; set; } = default!;

        [BindProperty]
        public List<SizeAttribute> SizeAttributes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var type = await _context.UniformType.FirstOrDefaultAsync(m => m.TypeID == id);
            if (type == null)
                return NotFound();

            Type = type;
            SizeAttributes = await _context.SizeAttribute.Where(a => a.TypeID == type.TypeID).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Type.TypeID);
                return Page();
            }

            bool duplicate = await _context.UniformType
                .AnyAsync(t => t.TypeID != Type.TypeID && t.TypeName.ToLower() == Type.TypeName.ToLower());
            if (duplicate)
            {
                ModelState.AddModelError("Type.TypeName", "A uniform type with this name already exists.");
                await OnGetAsync(Type.TypeID);
                return Page();
            }

            _context.Attach(Type).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var existingAttributes = await _context.SizeAttribute
                .Where(a => a.TypeID == Type.TypeID)
                .ToListAsync();

            var postedIds = SizeAttributes
                .Where(a => a.AttributeID != 0)
                .Select(a => a.AttributeID)
                .ToHashSet();

            var toDelete = existingAttributes
                .Where(a => !postedIds.Contains(a.AttributeID))
                .ToList();

            foreach (var attr in toDelete)
            {
                // Remove related entries from ReturnSize
                var returnSizes = await _context.ReturnSize
                    .Where(rs => rs.AttributeID == attr.AttributeID)
                    .ToListAsync();
                _context.ReturnSize.RemoveRange(returnSizes);

                // Remove related entries from StockSize
                var stockSizes = await _context.StockSize
                    .Where(ss => ss.AttributeID == attr.AttributeID)
                    .ToListAsync();
                _context.StockSize.RemoveRange(stockSizes);

                _context.SizeAttribute.Remove(attr);
            }

            foreach (var attr in SizeAttributes)
            {
                if (attr.AttributeID == 0 && !string.IsNullOrWhiteSpace(attr.AttributeName))
                {
                    attr.TypeID = Type.TypeID;
                    _context.SizeAttribute.Add(attr);
                }
                else
                {
                    var existingAttr = existingAttributes.FirstOrDefault(a => a.AttributeID == attr.AttributeID);
                    if (existingAttr != null && existingAttr.AttributeName != attr.AttributeName)
                    {
                        existingAttr.AttributeName = attr.AttributeName;
                        _context.SizeAttribute.Update(existingAttr);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./BrowseUniformTypes");
        }
    }
}