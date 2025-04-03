namespace SmartScheduledApi.Dtos;

public class CreateUserDto
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

public class UpdateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
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

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
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

public class UserTeamDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TeamRule { get; set; }
}

public class UserPermissionDto
{
    public int PermissionTeamId { get; set; }
    public string PermissionTeamName { get; set; }
    public string PermissionTeamRule { get; set; }
}

public class UserScheduleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public UserAssignmentDto Assignment { get; set; }
}

public class UserAssignmentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class UserTeamRule
{
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public int TeamRuleId { get; set; }
    public string TeamRuleName { get; set; }
}