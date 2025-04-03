namespace SmartScheduledApi.Dtos;

public class CreateAssignedDto
{
    public int ScheduledId { get; set; }
    public int AssignmentId { get; set; }
    public int MemberId { get; set; }
}

public class AssignedResponseDto
{
    public int Id { get; set; }
    public AssignedScheduleDto Schedule { get; set; }
    public AssignedAssignmentDto Assignment { get; set; }
    public AssignedMemberDto Member { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AssignedScheduleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AssignedAssignmentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public UserDto User { get; set; }
}

public class AssignedMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string TeamRule { get; set; }
}
