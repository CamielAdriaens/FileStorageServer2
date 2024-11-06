using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DAL;
using INTERFACES;
using LOGIC;

public static class ServiceExtensions
{
    public static void ConfigureAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQL Server
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Repository and Service layers
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    }
}
