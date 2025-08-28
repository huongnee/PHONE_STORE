using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;

using PHONE_STORE.API.Auth;                     // TokenService, LoginLockService
using PHONE_STORE.Application.Interfaces;       // IAuthService, ITokenService, IRefreshStore, IOtpStore, IEmailSender, IBrandRepository, IUserService, IUserRepository
using PHONE_STORE.Application.Options;          // JwtOptions
using PHONE_STORE.Application.Services;         // AuthService, UserService
using PHONE_STORE.Infrastructure.Auth;          // RedisRefreshStore, RedisOtpStore
using PHONE_STORE.Infrastructure.Data;          // PhoneDbContext
using PHONE_STORE.Infrastructure.Repositories;  // UserRepository, BrandRepository
using PHONE_STORE.Infrastructure.Email;         // SmtpEmailSender

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

// Brands
builder.Services.AddScoped<IBrandRepository, BrandRepository>();

// ================== Redis ==================
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration["Redis:Configuration"];
    o.InstanceName = builder.Configuration["Redis:InstanceName"];
});
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Configuration"]));

// Refresh token / login lock / OTP / Email
builder.Services.AddScoped<IRefreshStore, RedisRefreshStore>();
builder.Services.AddScoped<LoginLockService>();
builder.Services.AddScoped<IOtpStore, RedisOtpStore>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Email:Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

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

// ================== Pipeline ==================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Mvc");
app.UseRateLimiter();

// Tr? ProblemDetails cho l?i
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
