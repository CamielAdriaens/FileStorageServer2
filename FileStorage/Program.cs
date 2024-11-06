using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using LOGIC; // To access ConfigureAppServices extension
using INTERFACES;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configuration for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Register all required services via extension method
builder.Services.ConfigureAppServices(builder.Configuration);

// Add JWT Bearer authentication for Google
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Google:ClientId"], // Google Client ID from appsettings
            ValidIssuer = "https://accounts.google.com", // Google Issuer
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Use Google's public keys to validate the token
                var client = new HttpClient();
                var keys = client.GetStringAsync("https://www.googleapis.com/oauth2/v3/certs").Result;
                var jsonWebKeySet = new JsonWebKeySet(keys);
                return jsonWebKeySet.Keys;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication(); // Ensure this is added before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();
