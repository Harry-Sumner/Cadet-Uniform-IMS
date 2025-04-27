using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Collections;
using Cadet_Uniform_IMS.ViewModels;


[Authorize(Roles = "Cadet")]
public class ManageMeasurementsCadetModel : PageModel
{
    private readonly UserManager<IMS_User> _userManager;

    public ManageMeasurementsCadetModel(UserManager<IMS_User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public MeasurementView MeasurementView { get; set; }

    [TempData]
    public string Message { get; set; }

    public IMS_User user { get; set; }

    public async Task OnGetAsync()
    {
        user = await _userManager.GetUserAsync(User);
    }

    public async Task<IActionResult> OnPostEditMeasurementAsync()
    {
        if (user == null) return NotFound();

        user.Head = MeasurementView.Head;
        user.Height = MeasurementView.Height;
        user.Hips = MeasurementView.Hips;
        user.Chest = MeasurementView.Chest;
        user.Leg = MeasurementView.Leg;
        user.Neck = MeasurementView.Neck;
        user.Waist = MeasurementView.Waist;
        user.WaistKnee = MeasurementView.WaistKnee;
        user.Seat = MeasurementView.Seat;
        user.Shoe = MeasurementView.Shoe;

        await _userManager.UpdateAsync(user);
        Message = user.Name + " has been updated.";
        return RedirectToPage();
    }

}