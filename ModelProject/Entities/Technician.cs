namespace ModelProject.Entities;

// Not explicitly listed in the task's data model, but AssignedTechnicianId requires a
// referenceable entity to keep the relationship enforceable and queryable (e.g. for the
// technician filter). See README "Assumptions" for the rationale.
public class Technician
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
