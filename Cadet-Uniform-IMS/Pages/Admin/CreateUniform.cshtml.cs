using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cadet_Uniform_IMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateUniformModel : PageModel
    {
        private readonly Cadet_Uniform_IMS.Data.IMS_Context _context;

        public CreateUniformModel(Cadet_Uniform_IMS.Data.IMS_Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            Types = await _context.UniformType.ToListAsync();
            return Page();
            
        }

        [BindProperty]
        public Uniform Uniform { get; set; } = default!;
        public IList<UniformType> Types { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }

            foreach (var file in Request.Form.Files)
            {
                MemoryStream stream = new MemoryStream();
                file.CopyTo(stream);
                Uniform.ImageData = stream.ToArray();

                stream.Close();
                stream.Dispose();
            }

            bool uniformExists = await _context.Uniform
            .AnyAsync(t => t.Name.ToLower() == Uniform.Name.ToLower());

            if (uniformExists)
            {
                ModelState.AddModelError("Uniform.Name", "A uniform with this name already exists.");
                await OnGet();
                return Page();
            }

            var currentUniform = _context.Uniform.FromSqlRaw("SELECT * FROM Uniform")
                .OrderByDescending(b => b.UniformID)
                .FirstOrDefault();

            if (currentUniform == null)
            {
                Uniform.UniformID = 1;
            }
            else
            {
                Uniform.UniformID = currentUniform.UniformID + 1;
            }

            _context.Uniform.Add(Uniform);
            await _context.SaveChangesAsync();

            return RedirectToPage("./BrowseUniform_Types");
        }
    }
}
