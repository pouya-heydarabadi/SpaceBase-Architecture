using Identity.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Infrastructure.SqlServer.Configurations;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(x => x.HasIndex(x => x.PhoneNumber).IsUnique().IsDescending());
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<User> Users => Set<User>();
}