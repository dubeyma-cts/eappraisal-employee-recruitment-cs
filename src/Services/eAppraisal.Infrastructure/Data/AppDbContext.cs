using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.Entities;
using eAppraisal.Infrastructure.Identity;

namespace eAppraisal.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AppraisalCycle> AppraisalCycles => Set<AppraisalCycle>();
    public DbSet<Appraisal> Appraisals => Set<Appraisal>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<EmployeeFeedback> EmployeeFeedbacks => Set<EmployeeFeedback>();
    public DbSet<CtcSnapshot> CtcSnapshots => Set<CtcSnapshot>();
    public DbSet<StageHistory> StageHistories => Set<StageHistory>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Employee self-referencing manager
        builder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appraisal -> Employee
        builder.Entity<Appraisal>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Appraisals)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appraisal -> Manager Employee
        builder.Entity<Appraisal>()
            .HasOne(a => a.ManagerEmployee)
            .WithMany()
            .HasForeignKey(a => a.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appraisal -> Cycle
        builder.Entity<Appraisal>()
            .HasOne(a => a.Cycle)
            .WithMany(c => c.Appraisals)
            .HasForeignKey(a => a.CycleId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: Appraisal -> Comment
        builder.Entity<Comment>()
            .HasOne(c => c.Appraisal)
            .WithOne(a => a.ManagerComment)
            .HasForeignKey<Comment>(c => c.AppraisalId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: Appraisal -> EmployeeFeedback
        builder.Entity<EmployeeFeedback>()
            .HasOne(f => f.Appraisal)
            .WithOne(a => a.EmployeeFeedback)
            .HasForeignKey<EmployeeFeedback>(f => f.AppraisalId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: Appraisal -> CtcSnapshot
        builder.Entity<CtcSnapshot>()
            .HasOne(c => c.Appraisal)
            .WithOne(a => a.CtcSnapshot)
            .HasForeignKey<CtcSnapshot>(c => c.AppraisalId)
            .OnDelete(DeleteBehavior.Cascade);

        // StageHistory -> Appraisal
        builder.Entity<StageHistory>()
            .HasOne(s => s.Appraisal)
            .WithMany(a => a.StageHistories)
            .HasForeignKey(s => s.AppraisalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore TotalCTC computed property
        builder.Entity<CtcSnapshot>().Ignore(c => c.TotalCTC);

        // AuditEvent index for querying
        builder.Entity<AuditEvent>()
            .HasIndex(a => new { a.EntityType, a.EntityId });
        builder.Entity<AuditEvent>()
            .HasIndex(a => a.At);
    }
}
