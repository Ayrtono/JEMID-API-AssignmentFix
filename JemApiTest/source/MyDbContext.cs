using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext {

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Article> Articles { get; set; }

    // Dependency injection by decoupling usage from creation
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Article>()
        .HasKey(e => e.Code);
        
        modelBuilder.Entity<Article>()
        .HasIndex(e => e.Code)
        .IsUnique();
    }
}