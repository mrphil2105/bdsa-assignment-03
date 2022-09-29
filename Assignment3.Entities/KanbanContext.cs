using Assignment3.Core;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Entities;

public class KanbanContext : DbContext
{
    public KanbanContext(DbContextOptions<KanbanContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Task>()
            .Property(e => e.Title)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder
            .Entity<Task>()
            .Property(e => e.State)
            .HasConversion(
                v => v.ToString(),
                v => (State)Enum.Parse(typeof(State), v));
        

        modelBuilder
            .Entity<User>()
            .Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder
            .Entity<User>()
            .Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();
        
        modelBuilder
            .Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();
        

        modelBuilder
            .Entity<Tag>()
            .Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder
            .Entity<Tag>()
            .HasIndex(e => e.Name)
            .IsUnique();
    }
}
