using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Handlers.DataBase.Data;
using Server.Handlers.DataBase.Services;

namespace Server.Handlers.DataBase.Configuration;

public static class DatabaseConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql("Server=localhost;Database=RogueChat;User=root;Password=", new MySqlServerVersion(new Version(8,0,32))));
        
        services.AddTransient<UserServices>();

        //Automatically creates MySQL Database if not present one.
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
    }
}