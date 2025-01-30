using Microsoft.EntityFrameworkCore;

namespace CustomEventSourcing.Lib.Internal;

internal sealed class EventSourceDbContext(DbContextOptions<EventSourceDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => new
                {
                    e.StreamId,
                    e.Version
                }, "ix_stream_version")
                .IsUnique();
        });
    }
}