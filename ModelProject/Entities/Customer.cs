namespace ModelProject.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
