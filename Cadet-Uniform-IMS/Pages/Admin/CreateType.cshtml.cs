using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Cadet_Uniform_IMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateTypeModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public CreateTypeModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            return Page();
        }

        [BindProperty]
        public UniformType Type { get; set; } = default!;

        [BindProperty]
        public List<string> AttributeNames { get; set; } = new List<string>();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }

            bool typeExists = await _context.UniformType
                .AnyAsync(t => t.TypeName.ToLower() == Type.TypeName.ToLower());

            if (typeExists)
            {
                ModelState.AddModelError("Type.TypeName", "A uniform type with this name already exists.");
                await OnGet();
                return Page();
            }

            var currentType = _context.UniformType
                .OrderByDescending(b => b.TypeID)
                .FirstOrDefault();

            Type.TypeID = currentType?.TypeID + 1 ?? 1;

            _context.UniformType.Add(Type);
            await _context.SaveChangesAsync();

            int nextAttributeId = _context.SizeAttribute.Any()
                ? _context.SizeAttribute.Max(sa => sa.AttributeID) + 1
                : 1;

            foreach (var name in AttributeNames.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                var attribute = new SizeAttribute
                {
                    AttributeID = nextAttributeId++,
                    TypeID = Type.TypeID,
                    AttributeName = name
                };

                _context.SizeAttribute.Add(attribute);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./BrowseUniform_Types");
        }
    }
}