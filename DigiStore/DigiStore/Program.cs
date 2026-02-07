using DigiStore.Data;
using DigiStore.Services.CartServices;
using DigiStore.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


const string AdminScheme = "AdminScheme";
const string UserScheme = "UserScheme";


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = UserScheme;
    options.DefaultChallengeScheme = UserScheme;
})

.AddCookie(UserScheme, options =>
{
    options.Cookie.Name = ".DigiStore.User"; 
    options.LoginPath = "/Account/SendOtp"; 
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(2);
})

.AddCookie(AdminScheme, options =>
{
    options.Cookie.Name = ".DigiStore.Admin"; 
    options.LoginPath = "/Admin/Account/Login"; 
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1); 
});



builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});


app.MapControllerRoute(
    name: "areas", 
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();