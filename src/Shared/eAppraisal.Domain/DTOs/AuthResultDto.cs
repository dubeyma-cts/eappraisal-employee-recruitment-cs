namespace eAppraisal.Domain.DTOs;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Role { get; set; }
    public string? FullName { get; set; }
    public int? EmployeeId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
}
