namespace ContextEngineering.MemoryPOC.Repository;

using ContextEngineering.MemoryPOC.Entity;
using Microsoft.EntityFrameworkCore;

public class MemoryDbContext : DbContext
{
    public MemoryDbContext(DbContextOptions<MemoryDbContext> options) : base(options) { }

    public DbSet<LongTermMemory> LongTermMemories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LongTermMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MemoryType).HasConversion<string>();
        });
    }
}
