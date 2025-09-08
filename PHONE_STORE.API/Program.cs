using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PHONE_STORE.API.Auth;                     // TokenService, LoginLockService
using PHONE_STORE.Application.Interfaces;       // IAuthService, ITokenService, IRefreshStore, IOtpStore, IEmailSender, ...
using PHONE_STORE.Application.Options;          // JwtOptions
using PHONE_STORE.Application.Services;         // AuthService, UserService
using PHONE_STORE.Infrastructure.Auth;          // RedisRefreshStore, RedisOtpStore
using PHONE_STORE.Infrastructure.Data;          // PhoneDbContext
using PHONE_STORE.Infrastructure.Email;         // SmtpEmailSender
using PHONE_STORE.Infrastructure.Repositories;  // UserRepository, BrandRepository, CategoryRepository, ProductRepository, VariantRepository, ImageRepository, AttributeRepository, PriceRepository
using PHONE_STORE.Infrastructure.Services;
using Serilog;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ================== Serilog ==================
builder.Host.UseSerilog((ctx, lc) =>
{
    var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "api-.log");
    lc.MinimumLevel.Information()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Service", "PhoneStore.Api")
      .WriteTo.Console()
      .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
});

// ================== MVC & Swagger ==================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ================== ProblemDetails ==================
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});

// ================== DB ==================
builder.Services.AddDbContext<PhoneDbContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle"),
        oracle => oracle.MigrationsAssembly(typeof(PhoneDbContext).Assembly.FullName)));

// ================== JWT ==================
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

var validateKeys = (jwt.Keys ?? new())
    .GroupBy(k => k.Kid)
    .Select(g => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(g.First().Key)) { KeyId = g.Key })
    .ToList();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = validateKeys,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        o.IncludeErrorDetails = true;
    });

builder.Services.AddAuthorization();

// ================== DI (Application/Infrastructure) ==================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, TokenService>(); // stateless

// ========= Repositories =========
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IVariantRepository, VariantRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IAttributeRepository, AttributeRepository>();
builder.Services.AddScoped<IPriceRepository, PriceRepository>();

// using PHONE_STORE.Application.Interfaces;
// using PHONE_STORE.Infrastructure.Repositories;
// using PHONE_STORE.Infrastructure.Services;

builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

// ================== Redis & Email/OTP/Refresh ==================
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration["Redis:Configuration"];
    o.InstanceName = builder.Configuration["Redis:InstanceName"];
});
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Configuration"]));

builder.Services.AddScoped<IRefreshStore, RedisRefreshStore>();
builder.Services.AddScoped<LoginLockService>();
builder.Services.AddScoped<IOtpStore, RedisOtpStore>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Email:Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();


// ================== CORS ==================
var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(p => p.AddPolicy("Mvc",
    b => b.WithOrigins(allowed)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

// ================== Rate limiter cho /login ==================
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("login", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Log request t? b?n vi?t — ?? ngay sau Build là ?n
app.Use(async (ctx, next) =>
{
    var sid = ctx.Request.Headers["X-Session-Id"].FirstOrDefault() ?? ctx.Request.Cookies["sid"];
    var sub = ctx.User?.FindFirst("sub")?.Value;
    ctx.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("ReqLog")
        .LogInformation("REQ {Method} {Path} sid={Sid} sub={Sub}",
            ctx.Request.Method, ctx.Request.Path, sid, sub);
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(); // ??t trong else cho production
}

app.UseHttpsRedirection();
app.UseCors("Mvc");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


//var app = builder.Build();

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();                 // log ra console
//builder.Logging.AddDebug();

//builder.Services.AddDbContext<PhoneDbContext>(opt =>
//{
//    // ... c?u hình DbContext c?a b?n
//    opt.EnableSensitiveDataLogging();         // ?? ch? b?t môi tr??ng dev
//    opt.EnableDetailedErrors();
//});

//// middleware log nhanh
//app.Use(async (ctx, next) =>
//{
//    var sid = ctx.Request.Headers["X-Session-Id"].FirstOrDefault() ?? ctx.Request.Cookies["sid"];
//    var sub = ctx.User?.FindFirst("sub")?.Value;
//    ctx.RequestServices.GetRequiredService<ILoggerFactory>()
//        .CreateLogger("ReqLog")
//        .LogInformation("REQ {Method} {Path} sid={Sid} sub={Sub}",
//            ctx.Request.Method, ctx.Request.Path, sid, sub);
//    await next();
//});


//// ================== Pipeline ==================
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseCors("Mvc");
//app.UseRateLimiter();

//// Tr? ProblemDetails cho l?i
//app.UseExceptionHandler();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();
