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
    [Authorize(Roles ="Admin")]
    public class IndexModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public IndexModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public IList<Uniform> Uniform { get;set; } = default!;

        public IList<UniformType> Types { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            Types = await _context.UniformType.ToListAsync();
            Uniform = await _context.Uniform.ToListAsync();
            return Page();

        }

    }
}
