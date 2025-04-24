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

public class ManageMeasurementsModel : PageModel
{
    private readonly UserManager<IMS_User> _userManager;

    public ManageMeasurementsModel(UserManager<IMS_User> userManager)
    {
        _userManager = userManager;
    }

    public List<IMS_Cadet> Cadets { get; set; } = new();
    public List<IMS_Staff> Staff { get; set; } = new();

    [BindProperty]
    public MeasurementView MeasurementView { get; set; }

    [TempData]
    public string Message { get; set; }

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        Cadets = users.OfType<IMS_Cadet>().ToList();
        Staff = users.OfType<IMS_Staff>().ToList();
    }

    public async Task<IActionResult> OnPostEditMeasurementAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
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