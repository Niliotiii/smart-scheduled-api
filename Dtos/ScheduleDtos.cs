namespace SmartScheduledApi.Dtos;

public class CreateScheduleDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateScheduleDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ScheduleResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TeamDto Team { get; set; }
    public List<ScheduleAssignmentDto> Assignments { get; set; }
}

public class ScheduleAssignmentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<ScheduleUserDto> AssignedUsers { get; set; }
}

public class ScheduleUserDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
}
