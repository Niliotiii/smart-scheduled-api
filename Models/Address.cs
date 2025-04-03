namespace SmartScheduledApi.Models;

public class Address : BaseModel
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
