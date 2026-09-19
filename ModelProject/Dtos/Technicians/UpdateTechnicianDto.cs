using System.ComponentModel.DataAnnotations;

namespace ModelProject.Dtos.Technicians;

public class UpdateTechnicianDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public DateTime StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public bool IsActive { get; init; }
}
