using eAppraisal.Shared.Auth;
using eAppraisal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Shared.Data;

/// <summary>
/// Seeds demo data on first run. Safe to call on every startup (idempotent).
/// Demo password for all accounts: Demo@1234
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync()) return; // already seeded

        // All seeding is wrapped in a single transaction so a mid-seed crash
        // leaves the DB empty and the next startup reseeds cleanly.
        await using var tx = await db.Database.BeginTransactionAsync();

        // ── Employees ────────────────────────────────────────────────────────────────
        var mgr1 = new Employee
        {
            Name = "Amit Kumar", Email = "manager1@nano.com", Mobile = "9876543210",
            City = "Mumbai", Department = "Engineering", Gender = "Male",
            MaritalStatus = "Married", PhoneNumber = "022-12345678",
            Address = "101 Marine Drive, Mumbai",
            DateOfBirth = new DateTime(1980, 4, 15),
            DateOfJoining = new DateTime(2010, 6, 1),
            WorkExperienceYears = 14, PanNo = "AMITK1234A", CTC = 2500000
        };
        var mgr2 = new Employee
        {
            Name = "Priya Sharma", Email = "manager2@nano.com", Mobile = "9876543211",
            City = "Chennai", Department = "Finance", Gender = "Female",
            MaritalStatus = "Single", PhoneNumber = "044-98765432",
            Address = "22 Anna Nagar, Chennai",
            DateOfBirth = new DateTime(1985, 8, 22),
            DateOfJoining = new DateTime(2012, 3, 15),
            WorkExperienceYears = 12, PanNo = "PRIYA5678B", CTC = 2200000
        };
        var emp1 = new Employee
        {
            Name = "Raj Patel", Email = "emp1@nano.com", Mobile = "9876543212",
            City = "Mumbai", Department = "Engineering", Gender = "Male",
            MaritalStatus = "Single", PhoneNumber = "022-22334455",
            Address = "45 Bandra West, Mumbai",
            DateOfBirth = new DateTime(1992, 1, 10),
            DateOfJoining = new DateTime(2018, 7, 1),
            WorkExperienceYears = 6, PanNo = "RAJPT9012C", CTC = 1200000
        };
        var emp2 = new Employee
        {
            Name = "Neha Singh", Email = "emp2@nano.com", Mobile = "9876543213",
            City = "Mumbai", Department = "Engineering", Gender = "Female",
            MaritalStatus = "Married", PhoneNumber = "022-33445566",
            Address = "12 Andheri East, Mumbai",
            DateOfBirth = new DateTime(1994, 5, 18),
            DateOfJoining = new DateTime(2019, 2, 10),
            WorkExperienceYears = 5, PanNo = "NEHAS3456D", CTC = 1100000
        };
        var emp3 = new Employee
        {
            Name = "Vikram Das", Email = "emp3@nano.com", Mobile = "9876543214",
            City = "Chennai", Department = "Finance", Gender = "Male",
            MaritalStatus = "Married", PhoneNumber = "044-55667788",
            Address = "8 T Nagar, Chennai",
            DateOfBirth = new DateTime(1990, 11, 30),
            DateOfJoining = new DateTime(2017, 9, 5),
            WorkExperienceYears = 7, PanNo = "VIKRD7890E", CTC = 1300000
        };

        db.Employees.AddRange(mgr1, mgr2, emp1, emp2, emp3);
        await db.SaveChangesAsync();

        // Wire reporting structure
        emp1.ReportsToId = mgr1.Id;
        emp2.ReportsToId = mgr1.Id;
        emp3.ReportsToId = mgr2.Id;
        await db.SaveChangesAsync();

        // ── Users ─────────────────────────────────────────────────────────────
        const string pwd = "Demo@1234";
        db.Users.AddRange(
            new AppUser { Username = "hr@nano.com",       PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.HR },
            new AppUser { Username = "admin@nano.com",    PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.ITAdmin },
            new AppUser { Username = "manager1@nano.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.Manager,  EmployeeId = mgr1.Id },
            new AppUser { Username = "manager2@nano.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.Manager,  EmployeeId = mgr2.Id },
            new AppUser { Username = "emp1@nano.com",     PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.Employee, EmployeeId = emp1.Id },
            new AppUser { Username = "emp2@nano.com",     PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.Employee, EmployeeId = emp2.Id },
            new AppUser { Username = "emp3@nano.com",     PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd), Role = AppRoles.Employee, EmployeeId = emp3.Id }
        );
        await db.SaveChangesAsync();

        // ── Sample appraisals — coherent end-to-end demo flow ─────────────────
        //
        //  emp1 (Raj)   → mgr1 (Amit)  : AwaitingFinalAssessment
        //    Manager commented ✓  Employee submitted self-assessment ✓
        //    NEXT ACTION: manager1 submits final assessment
        //
        //  emp2 (Neha)  → mgr1 (Amit)  : AwaitingManagerComment
        //    HR initiated ✓
        //    NEXT ACTION: manager1 adds manager comment
        //
        //  emp3 (Vikram)→ mgr2 (Priya) : Completed
        //    Full cycle done — rating 4 awarded
        //    DEMO: everyone can see what a finished appraisal looks like
        //
        db.Appraisals.AddRange(
            // emp1: manager commented, employee replied → manager1 needs to submit final
            new Appraisal
            {
                EmployeeId = emp1.Id, ManagerId = mgr1.Id, Year = 2025,
                Status = AppraisalStatus.AwaitingFinalAssessment,
                ManagerComments = "Raj has shown strong technical skills this year. Delivered the API gateway migration ahead of schedule. Recommend focusing on documentation and knowledge transfer going forward.",
                SelfAssessmentInput = "I successfully led the API gateway migration, reducing latency by 20%. I have completed AWS Solutions Architect certification. My goal for next year is to mentor junior engineers and improve team documentation standards.",
                InitiatedAt = DateTime.UtcNow.AddDays(-30),
                ManagerCommentAt = DateTime.UtcNow.AddDays(-18),
                EmployeeInputAt = DateTime.UtcNow.AddDays(-5),
                InitiatedByHR = "hr@nano.com"
            },
            // emp2: HR initiated → manager1 needs to add comment (first action in the cycle)
            new Appraisal
            {
                EmployeeId = emp2.Id, ManagerId = mgr1.Id, Year = 2025,
                Status = AppraisalStatus.AwaitingManagerComment,
                InitiatedAt = DateTime.UtcNow.AddDays(-7),
                InitiatedByHR = "hr@nano.com"
            },
            // emp3: fully completed cycle — shows result view for emp3 and mgr2
            new Appraisal
            {
                EmployeeId = emp3.Id, ManagerId = mgr2.Id, Year = 2025,
                Status = AppraisalStatus.Completed,
                ManagerComments = "Vikram is a reliable team member with excellent analytical skills. Led the Q3 cost-optimisation initiative with measurable savings.",
                SelfAssessmentInput = "I successfully led the Q3 cost-reduction project, saving ₹12 L annually. I aim to take on cross-functional leadership in the next cycle.",
                FinalAssessment = "Vikram has consistently exceeded expectations. Strong delivery record and good team collaboration. Recommended for senior analyst role in the next cycle.",
                Rating = 4,
                InitiatedAt = DateTime.UtcNow.AddDays(-60),
                ManagerCommentAt = DateTime.UtcNow.AddDays(-50),
                EmployeeInputAt = DateTime.UtcNow.AddDays(-40),
                CompletedAt = DateTime.UtcNow.AddDays(-30),
                InitiatedByHR = "hr@nano.com"
            }
        );
        await db.SaveChangesAsync();

        // ── Seed audit trail ─────────────────────────────────────────────────
        db.AuditLogs.AddRange(
            new AuditLog { Actor = "hr@nano.com",       Role = AppRoles.HR,      Action = "APPRAISAL_INITIATED",         Details = $"Appraisal 2025 initiated for employee {emp3.Id} (Vikram)",  Timestamp = DateTime.UtcNow.AddDays(-60) },
            new AuditLog { Actor = "manager2@nano.com", Role = AppRoles.Manager, Action = "MANAGER_COMMENT_SUBMITTED",   Details = $"Manager comment submitted for appraisal of employee {emp3.Id}", Timestamp = DateTime.UtcNow.AddDays(-50) },
            new AuditLog { Actor = "emp3@nano.com",     Role = AppRoles.Employee,Action = "EMPLOYEE_INPUT_SUBMITTED",    Details = $"Self-assessment submitted by employee {emp3.Id}",           Timestamp = DateTime.UtcNow.AddDays(-40) },
            new AuditLog { Actor = "manager2@nano.com", Role = AppRoles.Manager, Action = "FINAL_ASSESSMENT_SUBMITTED",  Details = $"Final assessment completed for employee {emp3.Id}. Rating: 4", Timestamp = DateTime.UtcNow.AddDays(-30) },
            new AuditLog { Actor = "hr@nano.com",       Role = AppRoles.HR,      Action = "APPRAISAL_INITIATED",         Details = $"Appraisal 2025 initiated for employee {emp1.Id} (Raj)",     Timestamp = DateTime.UtcNow.AddDays(-30) },
            new AuditLog { Actor = "manager1@nano.com", Role = AppRoles.Manager, Action = "MANAGER_COMMENT_SUBMITTED",   Details = $"Manager comment submitted for appraisal of employee {emp1.Id}", Timestamp = DateTime.UtcNow.AddDays(-18) },
            new AuditLog { Actor = "emp1@nano.com",     Role = AppRoles.Employee,Action = "EMPLOYEE_INPUT_SUBMITTED",    Details = $"Self-assessment submitted by employee {emp1.Id}",           Timestamp = DateTime.UtcNow.AddDays(-5) },
            new AuditLog { Actor = "hr@nano.com",       Role = AppRoles.HR,      Action = "APPRAISAL_INITIATED",         Details = $"Appraisal 2025 initiated for employee {emp2.Id} (Neha)",    Timestamp = DateTime.UtcNow.AddDays(-7) }
        );
        await db.SaveChangesAsync();

        await tx.CommitAsync();
    }
}
