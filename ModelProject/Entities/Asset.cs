using ModelProject.Entities.Enums;

namespace ModelProject.Entities;

public class Asset
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    public Facility? Facility { get; set; }
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
