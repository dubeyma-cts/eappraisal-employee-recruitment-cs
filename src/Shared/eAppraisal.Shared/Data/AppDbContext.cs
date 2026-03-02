using eAppraisal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Shared.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser>  Users     { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Appraisal> Appraisals { get; set; }
    public DbSet<AuditLog>  AuditLogs  { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // AppUser
        b.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(50);
        });

        // Employee
        b.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.PanNo).HasMaxLength(10);
            e.Property(x => x.CTC).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.ReportsTo)
             .WithMany(x => x.DirectReports)
             .HasForeignKey(x => x.ReportsToId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Appraisal
        b.Entity<Appraisal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Employee)
             .WithMany(x => x.Appraisals)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Manager)
             .WithMany()
             .HasForeignKey(x => x.ManagerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog — append only
        b.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Actor).HasMaxLength(100);
            e.Property(x => x.Action).HasMaxLength(100);
        });
    }
}
