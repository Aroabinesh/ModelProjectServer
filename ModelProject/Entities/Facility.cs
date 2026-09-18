namespace ModelProject.Entities;

public class Facility
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
