namespace ModelProject.Dtos.Lookups;

public class TechnicianDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
