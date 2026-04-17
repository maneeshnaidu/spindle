using Microsoft.EntityFrameworkCore;
using Spindle.Backend.Domain;

namespace Spindle.Backend.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Flow> Flows => Set<Flow>();
    public DbSet<FlowStep> FlowSteps => Set<FlowStep>();
    public DbSet<Mapping> Mappings => Set<Mapping>();
    public DbSet<ScriptDefinition> Scripts => Set<ScriptDefinition>();
    public DbSet<Execution> Executions => Set<Execution>();
    public DbSet<ExecutionStep> ExecutionSteps => Set<ExecutionStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<FlowStepType>();

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(255);
            e.HasOne(x => x.Workspace).WithOne(x => x.User).HasForeignKey<Workspace>(x => x.UserId);
        });

        modelBuilder.Entity<Flow>(e =>
        {
            e.HasIndex(x => x.WebhookKey).IsUnique();
            e.HasMany(x => x.Steps).WithOne(x => x.Flow).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlowStep>(e =>
        {
            e.Property(x => x.Config).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Mapping>(e => { e.Property(x => x.Rules).HasColumnType("jsonb"); });
        modelBuilder.Entity<Execution>(e =>
        {
            e.Property(x => x.InputPayload).HasColumnType("jsonb");
            e.Property(x => x.OutputPayload).HasColumnType("jsonb");
            e.HasMany(x => x.Steps).WithOne(x => x.Execution).HasForeignKey(x => x.ExecutionId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ExecutionStep>(e =>
        {
            e.Property(x => x.InputPayload).HasColumnType("jsonb");
            e.Property(x => x.OutputPayload).HasColumnType("jsonb");
        });
    }
}
