using AuthAPI.Models;
using DataModels.Models.DbContext;
using DataModels.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure services (including UserService implementation)
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// Add DbContext using KitchenHeroDbContextFactory
builder.Services.AddDbContext<KitchenHeroDbContext>(options =>
{
    var factory = new KitchenHeroDbContextFactory();
    options.UseSqlServer(factory.CreateDbContext(new string[0]).Database.GetDbConnection().ConnectionString);
});

builder.Services.AddTransient<IUserRepository, UserRepository>(); // Replace with actual implementation

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();