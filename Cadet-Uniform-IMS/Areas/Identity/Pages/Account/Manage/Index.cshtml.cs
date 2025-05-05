using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Cadet_Uniform_IMS.Data;

namespace Cadet_Uniform_IMS.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IMS_User> _userManager;
        private readonly SignInManager<IMS_User> _signInManager;

        public IndexModel(
            UserManager<IMS_User> userManager,
            SignInManager<IMS_User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Rank")]
            public string Rank { get; set; }

            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Surname")]
            public string Surname { get; set; }

            [Display(Name = "Flight")]
            public string Flight { get; set; }
        }

        private async Task LoadAsync(IMS_User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            Input = new InputModel
            {
                Rank = user.Rank,
                FirstName = user.FirstName,
                Surname = user.Surname
            };

            if (user is IMS_Cadet cadet)
            {
                Input.Flight = cadet.Flight;
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(modelError.ErrorMessage);  
                }
                await LoadAsync(user);
                return Page();
            }

     
            user.Rank = Input.Rank;
            user.FirstName = Input.FirstName;
            user.Surname = Input.Surname;

            if (user is IMS_Cadet cadet)
            {
                cadet.Flight = Input.Flight;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {

                StatusMessage = "Unexpected error when updating profile.";
                foreach (var error in result.Errors)
                {
                    StatusMessage += $" Error: {error.Description}";
                }
                return RedirectToPage();
            }

    
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}