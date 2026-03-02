using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.DTOs;

public class CycleDto
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required, StringLength(20)]
    public string State { get; set; } = "Open";
}
