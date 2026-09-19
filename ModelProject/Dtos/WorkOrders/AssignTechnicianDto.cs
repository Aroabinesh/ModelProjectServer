using System.ComponentModel.DataAnnotations;

namespace ModelProject.Dtos.WorkOrders;

public class AssignTechnicianDto
{
    [Required]
    public int TechnicianId { get; init; }

    public DateTime? ScheduledStartDate { get; init; }
    public DateTime? ScheduledEndDate { get; init; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string ChangedBy { get; init; } = string.Empty;

    [Required]
    public string RowVersion { get; init; } = string.Empty;
}
