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
    public class DeleteModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public DeleteModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Uniform Uniform { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uniform = await _context.Uniform.FirstOrDefaultAsync(m => m.UniformID == id);

            if (uniform == null)
            {
                return NotFound();
            }
            else
            {
                Uniform = uniform;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uniform = await _context.Uniform.FindAsync(id);
            if (uniform != null)
            {
                Uniform = uniform;
                _context.Uniform.Remove(Uniform);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
