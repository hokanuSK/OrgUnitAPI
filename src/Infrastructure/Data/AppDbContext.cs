using CompanyStructure.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyStructure.Api.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<EmployeeOrgUnitAssignment> EmployeeOrgUnitAssignments => Set<EmployeeOrgUnitAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCompany(modelBuilder);
        ConfigureEmployee(modelBuilder);
        ConfigureOrgUnit(modelBuilder);
        ConfigureEmployeeOrgUnitAssignment(modelBuilder);
    }

    private static void ConfigureCompany(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Company>();

        entity.ToTable("Companies", t => t.HasTrigger("TR_Companies_SetUpdatedAt"));
        entity.HasKey(x => x.CompanyId);
        entity.Property(x => x.CompanyId).ValueGeneratedOnAdd();

        entity.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(x => x.Code)
            .IsUnique();

        entity.HasOne(x => x.DirectorEmployee)
            .WithMany()
            .HasForeignKey(x => new { x.CompanyId, x.DirectorEmployeeId })
            .HasPrincipalKey(x => new { x.CompanyId, x.EmployeeId })
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Employee>();

        entity.ToTable("Employees", t => t.HasTrigger("TR_Employees_SetUpdatedAt"));
        entity.HasKey(x => x.EmployeeId);
        entity.Property(x => x.EmployeeId).ValueGeneratedOnAdd();
        entity.HasAlternateKey(x => new { x.CompanyId, x.EmployeeId });

        entity.Property(x => x.Title)
            .HasMaxLength(50);

        entity.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Phone)
            .HasMaxLength(30);

        entity.Property(x => x.Email)
            .HasMaxLength(254)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(x => x.CompanyId);
        entity.HasIndex(x => new { x.CompanyId, x.Email })
            .IsUnique();

        entity.HasOne(x => x.Company)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureOrgUnit(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrgUnit>();

        entity.ToTable("OrgUnits", t =>
        {
            t.HasTrigger("TR_OrgUnits_SetUpdatedAt");
            t.HasTrigger("TR_OrgUnits_ValidateHierarchy");
            t.HasCheckConstraint("CK_OrgUnits_UnitType", "UnitType IN ('Division','Project','Department')");
            t.HasCheckConstraint("CK_OrgUnits_NotSelfParent", "ParentOrgUnitId IS NULL OR ParentOrgUnitId <> OrgUnitId");
        });
        entity.HasKey(x => x.OrgUnitId);
        entity.Property(x => x.OrgUnitId).ValueGeneratedOnAdd();
        entity.HasAlternateKey(x => new { x.CompanyId, x.OrgUnitId });

        entity.Property(x => x.UnitType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(x => x.CompanyId);
        entity.HasIndex(x => new { x.CompanyId, x.ParentOrgUnitId });
        entity.HasIndex(x => new { x.CompanyId, x.Code })
            .IsUnique();

        entity.HasOne(x => x.Company)
            .WithMany(x => x.OrgUnits)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.ParentOrgUnit)
            .WithMany(x => x.Children)
            .HasForeignKey(x => new { x.CompanyId, x.ParentOrgUnitId })
            .HasPrincipalKey(x => new { x.CompanyId, x.OrgUnitId })
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.LeaderEmployee)
            .WithMany(x => x.ManagedOrgUnits)
            .HasForeignKey(x => new { x.CompanyId, x.LeaderEmployeeId })
            .HasPrincipalKey(x => new { x.CompanyId, x.EmployeeId })
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureEmployeeOrgUnitAssignment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EmployeeOrgUnitAssignment>();

        entity.ToTable("EmployeeOrgUnitAssignments");
        entity.HasKey(x => new { x.EmployeeId, x.OrgUnitId, x.AssignedFrom });

        entity.Property(x => x.AssignedFrom)
            .HasDefaultValueSql("CONVERT(date, SYSUTCDATETIME())");

        entity.HasIndex(x => x.CompanyId);
        entity.HasIndex(x => x.OrgUnitId);

        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_EOUA_DateRange", "AssignedTo IS NULL OR AssignedTo >= AssignedFrom");
        });

        entity.HasOne(x => x.Employee)
            .WithMany(x => x.OrgUnitAssignments)
            .HasForeignKey(x => new { x.CompanyId, x.EmployeeId })
            .HasPrincipalKey(x => new { x.CompanyId, x.EmployeeId })
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.OrgUnit)
            .WithMany(x => x.EmployeeAssignments)
            .HasForeignKey(x => new { x.CompanyId, x.OrgUnitId })
            .HasPrincipalKey(x => new { x.CompanyId, x.OrgUnitId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
