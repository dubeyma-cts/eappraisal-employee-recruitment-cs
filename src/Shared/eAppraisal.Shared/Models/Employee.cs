namespace eAppraisal.Shared.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
    public string PassportNo { get; set; } = string.Empty;

    /// <summary>Stored encrypted; never exposed as raw. Always return masked.</summary>
    public string PanNo { get; set; } = string.Empty;

    public int WorkExperienceYears { get; set; }
    public int? ReportsToId { get; set; }
    public string Department { get; set; } = string.Empty;
    public decimal CTC { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee? ReportsTo { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = [];
    public ICollection<Appraisal> Appraisals { get; set; } = [];
}
