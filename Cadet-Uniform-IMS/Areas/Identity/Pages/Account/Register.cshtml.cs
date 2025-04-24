// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Cadet_Uniform_IMS.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Cadet_Uniform_IMS.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Admin")]

    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IMS_User> _signInManager;
        private readonly UserManager<IMS_User> _userManager;
        private readonly IUserStore<IMS_User> _userStore;
        private readonly IUserEmailStore<IMS_User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IMS_User> userManager,
            IUserStore<IMS_User> userStore,
            SignInManager<IMS_User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public CadetView CadetRegisterInput { get; set; }

        [BindProperty]
        public StaffView StaffRegisterInput { get; set; }

        [TempData]
        public string RegistrationMessage { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostCadetRegisterAsync(string returnUrl = null)
        {
            ModelState.Clear();
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = CreateCadet(); // Run function

                await _userStore.SetUserNameAsync(user, CadetRegisterInput.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, CadetRegisterInput.Email, CancellationToken.None);
                user.Rank = CadetRegisterInput.Rank;
                user.FirstName = CadetRegisterInput.FirstName;
                user.Surname = CadetRegisterInput.Surname;
                user.Flight = CadetRegisterInput.Flight;
                user.CadetNo = CadetRegisterInput.CadetNo;
                var result = await _userManager.CreateAsync(user, CadetRegisterInput.Password); // Store input from form

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "Cadet"); // Assign role

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(CadetRegisterInput.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = CadetRegisterInput.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        RegistrationMessage = "Account created successfully.";
                        return RedirectToPage("Register");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            OnGet();
            return Page();
        }

        public async Task<IActionResult> OnPostStaffRegisterAsync(string returnUrl = null)
        {
            ModelState.Clear(); // Clear previous errors
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = CreateStaff();
                // Create structure and store details from form
                await _userStore.SetUserNameAsync(user, StaffRegisterInput.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, StaffRegisterInput.Email, CancellationToken.None);
                user.Rank = StaffRegisterInput.Rank;
                user.FirstName = StaffRegisterInput.FirstName;
                user.Surname = StaffRegisterInput.Surname;
                user.StaffNo = StaffRegisterInput.StaffNo;
                var result = await _userManager.CreateAsync(user, StaffRegisterInput.Password);


                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "Staff");
                 
                    // Create user and account

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(StaffRegisterInput.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = StaffRegisterInput.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        RegistrationMessage = "Account created successfully.";
                        return RedirectToPage("Register");
                    }
                }
            }
            OnGet();
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IMS_Cadet CreateCadet()
        {
            try
            {
                return Activator.CreateInstance<IMS_Cadet>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IMS_Cadet)}'. " +
                    $"Ensure that '{nameof(IMS_Cadet)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
            //Create student account structure
        }

        private IMS_Staff CreateStaff()
        {
            try
            {
                return Activator.CreateInstance<IMS_Staff>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IMS_Staff)}'. " +
                    $"Ensure that '{nameof(IMS_Staff)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
            // Create the staff account structure
        }

        private IUserEmailStore<IMS_User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IMS_User>)_userStore;
        }
        //Store email
    }
}