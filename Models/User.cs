using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Models;

public class User : BaseModel
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Cpf { get; set; }
    public string Cellphone { get; set; }
    public string MotherName { get; set; }
    public string FatherName { get; set; }
    public string MotherCellphone { get; set; }
    public string FatherCellphone { get; set; }
    public Address Address { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
    public ICollection<Member> Members { get; set; }
}
