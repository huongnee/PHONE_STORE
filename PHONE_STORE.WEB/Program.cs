using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using PHONE_STORE.WEB.Infrastructure; // JwtCookieHandler, AutoRefreshHandler

var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

Log.Logger = logger;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithProperty("Service", "PhoneStore.Web"));

    builder.Services.AddControllersWithViews();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(o =>
        {
            o.LoginPath = "/Account/Login";
            o.LogoutPath = "/Account/Logout";
            o.ExpireTimeSpan = TimeSpan.FromHours(1);
            o.SlidingExpiration = true;
            o.Cookie.Name = "ps_auth";
            o.Cookie.HttpOnly = true;
            o.Cookie.SameSite = SameSiteMode.Lax;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

    builder.Services.AddAuthorization();

    // ===== HttpClient & Handlers =====
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7277";

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<JwtCookieHandler>();    // gắn Bearer từ cookie "access_token"
    builder.Services.AddTransient<AutoRefreshHandler>();  // bắt 401 -> gọi /api/auth/refresh

    // Client KHÔNG auth (AutoRefreshHandler dùng để gọi /refresh)
    builder.Services.AddHttpClient("ApiNoAuth", c =>
    {
        c.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    });

    // Client CHÍNH cho toàn bộ Web -> API (tên: "api")
    builder.Services.AddHttpClient("api", c =>
    {
        c.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    })
    .AddHttpMessageHandler<AutoRefreshHandler>()  // ngoài
    .AddHttpMessageHandler<JwtCookieHandler>();   // trong

    var app = builder.Build();

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

    app.UseSerilogRequestLogging();

    // Areas trước, default sau
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WEB terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
