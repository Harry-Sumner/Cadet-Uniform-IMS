using Microsoft.AspNetCore.Identity;

namespace Cadet_Uniform_IMS.Data
{
    public class SeedUsers
    {
        public static async Task Initialize(IMS_Context context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            string staffRole = "Staff";
            string cadetRole = "Cadet";
            string adminRole = "Admin";
            string password4all = "P@55word";

            if (await roleManager.FindByNameAsync(staffRole) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(staffRole));
            }

            if (await roleManager.FindByNameAsync(cadetRole) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(cadetRole));
            }

            if (await roleManager.FindByNameAsync(adminRole) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (await userManager.FindByNameAsync("cadet@hawardenaircadets.org") == null)
            {
                var user = new Cadet
                {
                    UserName = "cadet@hawardenaircadets.org",
                    Email = "cadet@hawardenaircadets.org",
                    FirstName = "Jane",
                    Surname = "Doe",
                    Rank = "Cadet",
                    CadetNo = "5122471234567890",
                    Flight = "A"
                };
                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddPasswordAsync(user, password4all);
                    await userManager.AddToRoleAsync(user, cadetRole);
                }
            }
            if (await userManager.FindByNameAsync("staff@hawardenaircadets.org") == null)
            {
                var user = new Staff
                {
                    UserName = "staff@hawardenaircadets.org",
                    Email = "staff@hawardenaircadets.org",
                    FirstName = "John",
                    Surname = "Doe",
                    Rank = "Sgt",
                    StaffNo = "123456789"
                };
                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddPasswordAsync(user, password4all);
                    await userManager.AddToRoleAsync(user, staffRole);
                }
            }
            if (await userManager.FindByNameAsync("admin@hawardenaircadets.org") == null)
            {
                var user = new Staff
                {
                    UserName = "admin@hawardenaircadets.org",
                    Email = "admin@hawardenaircadets.org",
                    FirstName = "Admin",
                    Surname = "Admin",
                    Rank = "Cpl",
                    StaffNo = "987654321"
                };
                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddPasswordAsync(user, password4all);
                    await userManager.AddToRoleAsync(user, adminRole);
                }
            }

        }
    }
}
