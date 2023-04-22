using Microsoft.EntityFrameworkCore;
using Server.Handlers.DataBase.Models;

namespace Server.Handlers.DataBase.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users>? Users { get; set; }
}