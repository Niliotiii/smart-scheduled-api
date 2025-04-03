using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Models;
using SmartScheduledApi.Data.Seeders;

namespace SmartScheduledApi.DataContext;

public class SmartScheduleApiContext : DbContext
{
    public SmartScheduleApiContext(DbContextOptions<SmartScheduleApiContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Scheduled> Scheduleds { get; set; }
    public DbSet<Assigned> Assigneds { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Invite> Invites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add seeders
        modelBuilder.Entity<User>().HasData(UserSeeder.Users);
        modelBuilder.Entity<Address>().HasData(UserSeeder.Addresses);

        // Configure global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Team>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Member>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Assignment>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Scheduled>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Assigned>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Address>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Invite>().HasQueryFilter(x => x.DeletedAt == null);  // Adicionar filtro para Invite

        modelBuilder.Entity<Member>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId);

        modelBuilder.Entity<Member>()
            .HasOne(m => m.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamId);

        // Remover configuração de Member para Assignment
        // modelBuilder.Entity<Assignment>()
        //    .HasOne(a => a.Member)
        //    .WithMany(m => m.Assignments)
        //    .HasForeignKey(a => a.MemberId);

        modelBuilder.Entity<Assigned>()
            .HasOne(a => a.Scheduled)
            .WithMany(s => s.Assigneds)
            .HasForeignKey(a => a.ScheduledId);

        modelBuilder.Entity<Assigned>()
            .HasOne(a => a.Assignment)
            .WithMany(a => a.Assigneds)
            .HasForeignKey(a => a.AssignmentId);

        // Nova configuração para Member em Assigned
        modelBuilder.Entity<Assigned>()
            .HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId);

        modelBuilder.Entity<Invite>()
            .HasOne(i => i.Team)
            .WithMany()
            .HasForeignKey(i => i.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invite>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invite>()
            .HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Adicionar relacionamento do Assignment com o Team
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Team)
            .WithMany()
            .HasForeignKey(a => a.TeamId);
    }

    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseModel baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        baseEntity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
