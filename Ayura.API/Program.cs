using System.Text;
using Ayura.API.Configuration;
using Ayura.API.Features.EmailVerification.Services;
using Ayura.API.Features.OTP.Services;
using Ayura.API.Features.Profile.Helpers.MailService;
using Ayura.API.Features.Profile.Services;
using Ayura.API.Global.MailService.Configuration;
using Ayura.API.Models.Configuration;
using Ayura.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddSingleton<IAppSettings>(sp =>
    sp.GetRequiredService<IOptions<AppSettings>>().Value);

builder.Services.Configure<AyuraDatabaseSettings>(
    builder.Configuration.GetSection(nameof(AyuraDatabaseSettings)));
builder.Services.AddSingleton<IAyuraDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<AyuraDatabaseSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("AyuraDatabaseSettings:ConnectionString")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileRetrieveService, ProfileRetrieveService>();
builder.Services.AddScoped<IProfileUpdateService, ProfileUpdateService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();

//Injecting Community Service
builder.Services.AddSingleton<CommunityService>();


// Mail Settings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailService, MailService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.SecretKey);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "https://localhost:7034",
            ValidAudience = "ayura-flutter-app",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();