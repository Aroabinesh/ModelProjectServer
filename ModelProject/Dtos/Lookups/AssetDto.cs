namespace ModelProject.Dtos.Lookups;

public class AssetDto
{
    public int Id { get; init; }
    public int FacilityId { get; init; }
    public string FacilityName { get; init; } = string.Empty;
    public string AssetCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AssetType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
