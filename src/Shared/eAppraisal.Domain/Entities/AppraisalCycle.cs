using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.Entities;

public class AppraisalCycle
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Cycle state: Open, Closed, Archived.
    /// </summary>
    [Required, StringLength(20)]
    public string State { get; set; } = "Open";

    public ICollection<Appraisal> Appraisals { get; set; } = new List<Appraisal>();
}
