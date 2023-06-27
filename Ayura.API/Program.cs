using Ayura.API.Models.Configuration;
using Ayura.API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// app settings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddSingleton<IAppSettings>(sp =>
    sp.GetRequiredService<IOptions<AppSettings>>().Value);
// database settings
builder.Services.Configure<AyuraDatabaseSettings>(
    builder.Configuration.GetSection(nameof(AyuraDatabaseSettings)));
builder.Services.AddSingleton<IAyuraDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<AyuraDatabaseSettings>>().Value);
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("AyuraDatabaseSettings:ConnectionString")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();