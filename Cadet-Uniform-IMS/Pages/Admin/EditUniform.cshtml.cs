using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cadet_Uniform_IMS.Data;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    public class EditUniformModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public EditUniformModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Uniform Uniform { get; set; } = default!;

        public IList<UniformType> Types { get; set; } = default!;
        public UniformType Type { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uniform =  await _context.Uniform.FirstOrDefaultAsync(m => m.UniformID == id);
            if (uniform == null)
            {
                return NotFound();
            }
            Uniform = uniform;
            var type = await _context.UniformType.FirstOrDefaultAsync(m => m.TypeID == uniform.TypeID);
            if (type == null) {
                return Page();
            }
            Type = type;

            Types = await _context.UniformType.ToListAsync();
            Types.Remove(type);

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Uniform.UniformID);
                return Page();
            }

            bool duplicate = await _context.Uniform
            .AnyAsync(u => u.UniformID != Uniform.UniformID && u.Name.ToLower() == Uniform.Name.ToLower());
            if (duplicate)
            {
                ModelState.AddModelError("Uniform.Name", "A uniform with this name already exists.");
                await OnGetAsync(Uniform.UniformID);
                return Page();
            }

            _context.Attach(Uniform).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UniformExists(Uniform.UniformID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./BrowseUniform_Types");
        }

        private bool UniformExists(int id)
        {
            return _context.Uniform.Any(e => e.UniformID == id);
        }
    }
}
