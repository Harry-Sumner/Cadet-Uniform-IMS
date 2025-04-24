using Cadet_Uniform_IMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IMS_Context");
builder.Services.AddDbContext<IMS_Context>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IMS_User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<IMS_Context>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmins", policy => policy.RequireRole("Admin"));
});

//Added authorisation so only those with admin role are able to access pages contained in Admin folder
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeFolder("/Admin", "RequireAdmins");
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<IMS_Context>();
    context.Database.Migrate();
    var userMgr = services.GetRequiredService<UserManager<IMS_User>>();
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
    SeedUsers.Initialize(context, userMgr, roleMgr).Wait();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
