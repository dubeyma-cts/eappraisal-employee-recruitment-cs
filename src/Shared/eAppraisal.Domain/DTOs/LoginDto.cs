using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.DTOs;

public class LoginDto
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
