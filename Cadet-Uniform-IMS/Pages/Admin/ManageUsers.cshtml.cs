using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cadet_Uniform_IMS.Data;

namespace Cadet_Uniform_IMS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<IMS_User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageUsersModel(UserManager<IMS_User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public List<IMS_Cadet> Cadets { get; set; } = new();

        [BindProperty]
        public List<IMS_Staff> Staff { get; set; } = new();

        public Dictionary<string, bool> Admins { get; set; } = new();

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            Cadets = users.OfType<IMS_Cadet>().ToList();
            Staff = users.OfType<IMS_Staff>().ToList();
            foreach (var user in Staff)
            {
                Admins[user.Id] = await _userManager.IsInRoleAsync(user, "Admin");
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await OnGetAsync(); // Repopulate the model
                return Page();
            }

            Message = user.Name + " password has been updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            Message = user.Name + " role has been updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await OnGetAsync(); // Repopulate the model
                return Page();
            }
            Message = user.Name + " account has been deleted.";
            return RedirectToPage();
        }
    }
}