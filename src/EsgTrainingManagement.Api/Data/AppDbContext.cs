using EsgTrainingManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EsgTrainingManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<JobRole> JobRoles => Set<JobRole>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingRequirement> TrainingRequirements => Set<TrainingRequirement>();
    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var utc = new ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        model.Entity<JobRole>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(100).IsUnicode(false);
            e.Property(x => x.Description).HasMaxLength(300).IsUnicode(false);
            e.Property(x => x.Department).HasConversion<int>();
            e.HasIndex(x => x.Department);
        });
        model.Entity<Employee>(e =>
        {
            e.Property(x => x.Id).HasColumnType("RAW(16)").ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(150).IsUnicode(false);
            e.Property(x => x.EmployeeNumber).HasMaxLength(30).IsUnicode(false);
            e.Property(x => x.Email).HasMaxLength(150).IsUnicode(false);
            e.Property(x => x.HireDate).HasConversion(utc);
            e.Property(x => x.IsActive).HasConversion<int>().HasColumnType("NUMBER(1)");
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => new { x.IsActive, x.JobRoleId });
            e.HasOne(x => x.JobRole).WithMany().HasForeignKey(x => x.JobRoleId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<Training>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(150).IsUnicode(false);
            e.Property(x => x.Description).HasMaxLength(500).IsUnicode(false);
            e.Property(x => x.EsgPillar).HasConversion<int>();
            e.HasIndex(x => new { x.EsgPillar, x.Title });
        });
        model.Entity<TrainingRequirement>(e =>
        {
            e.HasKey(x => new { x.JobRoleId, x.TrainingId });
            e.Property(x => x.IsMandatory).HasConversion<int>().HasColumnType("NUMBER(1)");
            e.HasOne(x => x.JobRole).WithMany(x => x.TrainingRequirements).HasForeignKey(x => x.JobRoleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Training).WithMany().HasForeignKey(x => x.TrainingId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<TrainingRecord>(e =>
        {
            e.Property(x => x.EmployeeId).HasColumnType("RAW(16)");
            e.Property(x => x.CompletedAt).HasConversion(utc);
            e.Property(x => x.ExpiresAt).HasConversion(utc);
            e.Property(x => x.Score).HasPrecision(5, 2).HasColumnType("NUMBER(5,2)");
            e.Property(x => x.CertificateCode).HasMaxLength(60).IsUnicode(false);
            e.HasIndex(x => new { x.EmployeeId, x.TrainingId, x.CompletedAt });
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Training).WithMany().HasForeignKey(x => x.TrainingId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
