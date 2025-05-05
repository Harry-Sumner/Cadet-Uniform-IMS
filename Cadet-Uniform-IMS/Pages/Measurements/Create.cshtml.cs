using System.Threading.Tasks;
using Cadet_Uniform_IMS.Data;
using Cadet_Uniform_IMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cadet_Uniform_IMS.Pages.Measurements
{
    [Authorize]
    public class CreateModel : PageModel
    {
        [BindProperty]
        public MeasurementView Measurements { get; set; } = new();

        private readonly UserManager<IMS_User> _userManager;
        private readonly SignInManager<IMS_User> _signInManager;

        public CreateModel(UserManager<IMS_User> userManager, SignInManager<IMS_User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            Measurements = new MeasurementView
            {
                Height = user.Height,
                Head = user.Head,
                Neck = user.Neck,
                Chest = user.Chest,
                Leg = user.Leg,
                WaistKnee = user.WaistKnee,
                Waist = user.Waist,
                Hips = user.Hips,
                Seat = user.Seat,
                Shoe = user.Shoe
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Manually update fields
            user.Height = Measurements.Height;
            user.Head = Measurements.Head;
            user.Neck = Measurements.Neck;
            user.Chest = Measurements.Chest;
            user.Leg = Measurements.Leg;
            user.WaistKnee = Measurements.WaistKnee;
            user.Waist = Measurements.Waist;
            user.Hips = Measurements.Hips;
            user.Seat = Measurements.Seat;
            user.Shoe = Measurements.Shoe;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("/Index");
        }
    }
}