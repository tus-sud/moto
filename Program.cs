using MotoShopWeb.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
var rawConnStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=WebBanXeMoto;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
if (rawConnStr.StartsWith("Server=\\MSSQLLocalDB", StringComparison.OrdinalIgnoreCase))
{
    rawConnStr = rawConnStr.Replace("Server=\\MSSQLLocalDB", "Server=(localdb)\\MSSQLLocalDB", StringComparison.OrdinalIgnoreCase);
}
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(rawConnStr));
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.Cookie.Name="MotoShopABG.Cart"; o.Cookie.HttpOnly=true; o.Cookie.IsEssential=true; o.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest; o.IdleTimeout=TimeSpan.FromHours(2); });
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<CustomerCookieEvents>();
builder.Services.AddScoped<StaffCookieEvents>();
builder.Services.AddScoped<VehicleAccess>();
builder.Services.AddScoped<IAuthorizationHandler, VehiclePermissionHandler>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MotoShopWeb.Staff";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Staff/Login";
        options.AccessDeniedPath = "/Staff/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.EventsType = typeof(StaffCookieEvents);
    }).AddCookie("Customer", options => {
        options.Cookie.Name="MotoShopABG.Customer";
        options.Cookie.HttpOnly=true; options.Cookie.SecurePolicy=CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite=SameSiteMode.Lax; options.LoginPath="/Customer/Login";
        options.AccessDeniedPath="/Customer/Login"; options.ExpireTimeSpan=TimeSpan.FromHours(2);
        options.EventsType=typeof(CustomerCookieEvents);
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser()
        .RequireRole("Admin").RequireClaim("PasswordChanged", "true"));
    options.AddPolicy("StaffReady", policy => policy.RequireAuthenticatedUser()
        .RequireClaim("PasswordChanged", "true"));
    foreach (var code in VehiclePermissions.All)
        options.AddPolicy(code, policy => policy.RequireAuthenticatedUser()
            .RequireClaim("PasswordChanged", "true").AddRequirements(new VehiclePermissionRequirement(code)));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("staff-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("vi-VN").AddSupportedCultures("vi-VN").AddSupportedUICultures("vi-VN"));
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Tự động tạo Database WebBanXeMoto, đồng bộ các bảng còn thiếu, nạp 18 mẫu xe mẫu và khởi tạo tài khoản admin trên máy phát triển.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Canh bao dong bo schema DB: " + ex.Message);
    }

    try
    {
        var seedSqlPath = Path.Combine(app.Environment.ContentRootPath, "DuLieuMau", "01_NHAP_18_MAU_XE.sql");
        if (!await db.Xes.AnyAsync() && File.Exists(seedSqlPath))
        {
            var seedSql = await File.ReadAllTextAsync(seedSqlPath);
            await db.Database.ExecuteSqlRawAsync(seedSql);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Canh bao nap 18 mau xe: " + ex.Message);
    }

    try
    {
        await AdminBootstrap.InitializeAsync(db, app.Configuration["AdminBootstrap:Password"] ?? "Admin@12345678");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Canh bao khoi tao Admin: " + ex.Message);
    }
}

Console.WriteLine("Now listening on: http://localhost:5158");
app.Run();
