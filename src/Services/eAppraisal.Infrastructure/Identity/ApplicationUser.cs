using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [StringLength(200)]
    public string? FullName { get; set; }

    [StringLength(50)]
    public string? AppRole { get; set; }

    public int? EmployeeId { get; set; }

    public int FailedLoginAttempts { get; set; }

    public bool IsManuallyLocked { get; set; }
}
