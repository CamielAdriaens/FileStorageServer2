using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DAL;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000", "https://localhost:3000") // Allow both HTTP and HTTPS if needed
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials(); // For cookies or credentials if needed
        });
});

// Configure application services (includes DbContext, repositories, and services)
builder.Services.ConfigureAppServices(builder.Configuration);

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com", // For Google token issuer validation
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Google:ClientId"], // The Google ClientId for validation
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),

            // Map claims so they can be accessed with standard ClaimTypes
            NameClaimType = ClaimTypes.NameIdentifier, // Maps the 'sub' claim to the NameIdentifier
            RoleClaimType = ClaimTypes.Role
        };

        // Optional: Require HTTPS metadata retrieval
        options.RequireHttpsMetadata = false; // Set to true in production
    });

// Optional: Add authorization policies if needed
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy =>
        policy.RequireClaim(ClaimTypes.NameIdentifier)); // Require NameIdentifier for user actions
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Ensure CORS is applied before Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
