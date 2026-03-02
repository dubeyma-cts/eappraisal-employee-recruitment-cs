using Microsoft.EntityFrameworkCore;
using eAppraisal.Domain.Entities;

namespace eAppraisal.Application.Contracts;

public interface IAppDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<AppraisalCycle> AppraisalCycles { get; }
    DbSet<Appraisal> Appraisals { get; }
    DbSet<Comment> Comments { get; }
    DbSet<EmployeeFeedback> EmployeeFeedbacks { get; }
    DbSet<CtcSnapshot> CtcSnapshots { get; }
    DbSet<StageHistory> StageHistories { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
