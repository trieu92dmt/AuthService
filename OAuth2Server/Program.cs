using Infracstructure.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OAuth2Server.Applications.DTOs.Auth;
using OpenIddict.Abstractions;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.SetMinimumLevel(LogLevel.Debug);
// Add db context
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Enable OpenIddict entities trong DbContext để lưu dữ liệu OAuth2/OpenID
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddDefaultTokenProviders();

// Get certificate settings from configuration (appsettings.json)
var certSettings = builder.Configuration
    .GetSection("Certificates:Signing")
    .Get<CertificateSettings>();

// OpenIddict
builder.Services.AddOpenIddict()
    // Dùng EF Core + AuthenticationDbContext để lưu dữ liệu
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<AuthenticationDbContext>();
    })
    .AddServer(options =>
    {
        options.AllowAuthorizationCodeFlow()                            // Flow chuẩn OAuth2 dùng cho
               .RequireProofKeyForCodeExchange();                       // PKCE Bắt buộc bảo mật cao hơn (chống bị đánh cắp code)
        options.SetAuthorizationEndpointUris("/connect/authorize")      // Endpoint: login + xin quyền
               .SetTokenEndpointUris("/connect/token")                  // Endpoint: đổi code → access token
               .SetUserInfoEndpointUris("/connect/userinfo");           // Endpoint: lấy thông tin user từ access token
        options.AllowRefreshTokenFlow();                                // Cho phép dùng refresh token để lấy access token mới khi access token cũ hết hạn
        options.RegisterScopes("api", "profile", "email");              // Đăng ký scope "api" để client có thể xin quyền truy cập vào API
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));       // Access token có thời hạn 30 phút
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));          // Refresh token có thời hạn 7 ngày
        //options.AddEncryptionCertificate(
        //                        new X509Certificate2(
        //                            certSettings.Path,
        //                            certSettings.Password))             // Chứng chỉ dùng để mã hóa access token (JWT) - bắt buộc trong production, có thể dùng chứng chỉ phát triển trong dev
        //        .AddSigningCertificate(
        //                        new X509Certificate2(
        //                            certSettings.Path,
        //                            certSettings.Password));
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();                     // Chứng chỉ phát triển (dùng cho môi trường dev, không dùng cho production)
        options.UseAspNetCore()                                         // Dùng để custom logic xử lý request/response của OpenIddict trong pipeline ASP.NET Core
               .EnableAuthorizationEndpointPassthrough();                // Bật khi muốn customer logic login
                                                                         //.EnableTokenEndpointPassthrough()                        // Bật khi muốn custom logic xử lý token request (ví dụ: thêm claims vào token)
                                                                         //.EnableUserInfoEndpointPassthrough();                    // Bật khi muốn custom logic xử lý userinfo request (ví dụ: lấy thêm thông tin user từ database)
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        Console.WriteLine("🔥 HIT LOGIN REDIRECT");

        var returnUrl = context.Request.Path + context.Request.QueryString;

        context.Response.Redirect(
            $"http://localhost:3000/login?returnUrl={Uri.EscapeDataString(returnUrl)}"
        );

        return Task.CompletedTask;
    };

    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000","http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // 🔥 bắt buộc nếu dùng cookie
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed client application (react-client) vào database nếu chưa tồn tại
using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("react-client") == null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "react-client",
            DisplayName = "React App",
            RedirectUris = { new Uri("http://localhost:3001/callback") },

            Permissions =
    {
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.Token,

        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.ResponseTypes.Code,

        OpenIddictConstants.Permissions.Scopes.Profile,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Prefixes.Scope + "api"
    },

            Requirements =
    {
        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange // 👈 QUAN TRỌNG
    }
        });
    }
}

app.Run();
