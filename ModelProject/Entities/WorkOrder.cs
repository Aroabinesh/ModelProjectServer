using System.ComponentModel.DataAnnotations;
using ModelProject.Entities.Enums;

namespace ModelProject.Entities;

public class WorkOrder
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    // Denormalized from Asset.FacilityId at creation time. This lets the facility filter and
    // the FacilityId/Status/CreatedAt query (see SQL Server challenge in the task PDF) hit a
    // single covering index instead of joining through Assets for every list/filter request.
    public int FacilityId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.New;

    public int? AssignedTechnicianId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Asset? Asset { get; set; }
    public Facility? Facility { get; set; }
    public Technician? AssignedTechnician { get; set; }
    public ICollection<WorkOrderHistory> History { get; set; } = new List<WorkOrderHistory>();
}
