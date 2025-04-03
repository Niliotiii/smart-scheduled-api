namespace SmartScheduledApi.Dtos;

public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Cpf { get; set; }
    public string Cellphone { get; set; }
    public string MotherName { get; set; }
    public string FatherName { get; set; }
    public string MotherCellphone { get; set; }
    public string FatherCellphone { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public class ResetPasswordDto
{
    public int UserId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
