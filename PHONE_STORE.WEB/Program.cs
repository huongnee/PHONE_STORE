using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using PHONE_STORE.WEB.Infrastructure; // JwtCookieHandler, AutoRefreshHandler

// ===== Serilog bootstrap (để bắt log ngay từ đầu) =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog tích hợp vào hosting
    builder.Host.UseSerilog((ctx, lc) =>
        lc.MinimumLevel.Information()
          .Enrich.FromLogContext()
          .Enrich.WithProperty("Service", "PhoneStore.Web")
          .WriteTo.Console()
          .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

    builder.Services.AddControllersWithViews();

    // ===== Cookie auth cho MVC =====
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
            o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });
    builder.Services.AddAuthorization();

    // ===== HttpClient (WEB -> API) =====
    var apiBase =
        builder.Configuration["Api:BaseUrl"]
        ?? builder.Configuration["ApiBaseUrl"]
        ?? "https://localhost:7277";

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<JwtCookieHandler>();
    builder.Services.AddTransient<AutoRefreshHandler>();

    // Client có JWT & tự refresh khi 401
    builder.Services.AddHttpClient("api", c =>
    {
        c.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    })
    .AddHttpMessageHandler<JwtCookieHandler>()
    .AddHttpMessageHandler<AutoRefreshHandler>();

    // Client không auth (dùng cho /auth/login, /auth/refresh, …)
    builder.Services.AddHttpClient("ApiNoAuth", c =>
    {
        c.BaseAddress = new Uri(apiBase.TrimEnd('/') + "/");
    });

    var app = builder.Build();

    // Cấp 'sid' cho khách (để API nhận diện giỏ hàng guest theo session)
    app.Use(async (ctx, next) =>
    {
        if (!ctx.Request.Cookies.TryGetValue("sid", out var sid) || string.IsNullOrWhiteSpace(sid))
        {
            sid = Guid.NewGuid().ToString("N");
            var isDev = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
            ctx.Response.Cookies.Append("sid", sid, new CookieOptions
            {
                HttpOnly = false,               // cho JS đọc nếu cần
                Secure = !isDev,                // prod: true
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }
        await next();
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging(); // log mỗi request của WEB

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "product_slug",
        pattern: "p/{slug}",
        defaults: new { controller = "Catalog", action = "Details" });

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
