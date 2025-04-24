using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Mvc;

public class ManageMeasurementsModel : PageModel
{
    private readonly UserManager<IMS_User> _userManager;

    public ManageMeasurementsModel(UserManager<IMS_User> userManager)
    {
        _userManager = userManager;
    }

    public List<IMS_Cadet> Cadets { get; set; } = new();
    public List<IMS_Staff> Staff { get; set; } = new();

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        Cadets = users.OfType<IMS_Cadet>().ToList();
        Staff = users.OfType<IMS_Staff>().ToList();
    }

}