using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using eAppraisal.Domain.Entities;
using eAppraisal.Infrastructure.Identity;

namespace eAppraisal.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        if (await db.Employees.AnyAsync())
            return; // Already seeded

        // 1. Create employees
        var manager = new Employee
        {
            FirstName = "Ramesh", LastName = "Kumar", Email = "ramesh.kumar@nanotechnologies.com",
            Department = "Engineering", City = "Mumbai", DateOfJoining = new DateTime(2015, 3, 1),
            PanNo = "ABCDE1234F", Gender = "Male", MobileNo = "9876543210"
        };
        db.Employees.Add(manager);
        await db.SaveChangesAsync();

        var emp1 = new Employee
        {
            FirstName = "Priya", LastName = "Sharma", Email = "priya.sharma@nanotechnologies.com",
            Department = "Engineering", City = "Pune", DateOfJoining = new DateTime(2019, 7, 15),
            PanNo = "FGHIJ5678K", Gender = "Female", MobileNo = "9876543211",
            ManagerId = manager.Id
        };
        var emp2 = new Employee
        {
            FirstName = "Anil", LastName = "Patel", Email = "anil.patel@nanotechnologies.com",
            Department = "QA", City = "Bangalore", DateOfJoining = new DateTime(2020, 1, 10),
            PanNo = "KLMNO9012P", Gender = "Male", MobileNo = "9876543212",
            ManagerId = manager.Id
        };
        var hr = new Employee
        {
            FirstName = "Sunita", LastName = "Rao", Email = "sunita.rao@nanotechnologies.com",
            Department = "HR", City = "Mumbai", DateOfJoining = new DateTime(2016, 5, 20),
            PanNo = "QRSTU3456V", Gender = "Female", MobileNo = "9876543213"
        };
        db.Employees.AddRange(emp1, emp2, hr);
        await db.SaveChangesAsync();

        // 2. Create appraisal cycle
        var cycle = new AppraisalCycle
        {
            Name = "2025 Annual Appraisal",
            StartDate = new DateTime(2025, 4, 1),
            EndDate = new DateTime(2025, 6, 30),
            State = "Open"
        };
        db.AppraisalCycles.Add(cycle);
        await db.SaveChangesAsync();

        // 3. Create Identity users
        async Task CreateUser(string email, string password, string fullName, string role, int? empId)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                AppRole = role,
                EmployeeId = empId
            };
            await userManager.CreateAsync(user, password);
        }

        await CreateUser("admin@nanotechnologies.com", "Admin@123", "System Administrator", "Admin", null);
        await CreateUser("sunita.rao@nanotechnologies.com", "Hr@12345", "Sunita Rao", "HR", hr.Id);
        await CreateUser("ramesh.kumar@nanotechnologies.com", "Manager@123", "Ramesh Kumar", "Manager", manager.Id);
        await CreateUser("priya.sharma@nanotechnologies.com", "Employee@123", "Priya Sharma", "Employee", emp1.Id);
        await CreateUser("anil.patel@nanotechnologies.com", "Employee@123", "Anil Patel", "Employee", emp2.Id);
    }
}
