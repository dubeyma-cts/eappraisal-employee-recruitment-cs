using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? PersonalPhone { get; set; }

    [StringLength(20)]
    public string? MobileNo { get; set; }

    [StringLength(256)]
    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? MaritalStatus { get; set; }

    public DateTime DateOfJoining { get; set; }

    [StringLength(20)]
    public string? PassportNo { get; set; }

    [StringLength(20)]
    public string? PanNo { get; set; }

    public int? WorkExperience { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    public int? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    public Employee? Manager { get; set; }

    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    public ICollection<Appraisal> Appraisals { get; set; } = new List<Appraisal>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
