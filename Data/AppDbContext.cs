using Microsoft.EntityFrameworkCore;
using Bramki.Models;

namespace Bramki.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

    public DbSet<Person> People => Set<Person>();
    public DbSet<GatePrivilege> GatePrivileges => Set<GatePrivilege>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(e =>
        {
            e.ToTable("DUser", "dbo");
            e.HasKey(x => x.ID);
            e.Property(x => x.ID).HasColumnName("ID");
            e.Property(x => x.Name).HasColumnName("Name").IsRequired().HasMaxLength(200);
            e.Property(x => x.CardNumber).HasColumnName("CardNumber").HasMaxLength(50);
            e.Property(x => x.CardNumber2).HasColumnName("CardNumber2").HasMaxLength(50);
            e.Property(x => x.ERPID).HasColumnName("ERPID").HasMaxLength(10);
            e.Property(x => x.IsActive).HasColumnName("IsActive");

            e.Ignore(x => x.FirstName);
            e.Ignore(x => x.Surname);

            e.HasQueryFilter(p =>
                p.IsActive
                && p.ERPID != null && p.ERPID != ""
                && (p.CardNumber == null || p.CardNumber == "" || !EF.Functions.Like(p.CardNumber, "%[^0-9]%"))
                && p.GatePrivileges.Any()
            );
        });

        modelBuilder.Entity<GatePrivilege>(e =>
        {
            e.ToTable("DGates", "dbo");
            e.HasKey(x => x.GatesId);

            e.Property(x => x.GatesId).HasColumnName("gates_id");
            e.Property(x => x.DUserId).HasColumnName("duser_id");
            e.Property(x => x.GatesForklifts).HasColumnName("gates_forklifts");
            e.Property(x => x.GatesCranes).HasColumnName("gates_cranes");
            e.Property(x => x.GatesGantries).HasColumnName("gates_gantries");

            e.HasOne(g => g.Person)
             .WithMany(p => p.GatePrivileges)
             .HasForeignKey(g => g.DUserId)
             .HasPrincipalKey(p => p.ID);
        });
    }
}