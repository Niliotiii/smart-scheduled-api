namespace SmartScheduledApi.Dtos;

public class CreateAssignmentDto
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class UpdateAssignmentDto
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class AssignmentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; }
}

public class AssignmentDetailResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public List<AssignmentMemberDto> Members { get; set; } = new List<AssignmentMemberDto>();
}

public class AssignmentMembersDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<AssignmentMemberDto> Members { get; set; } = new List<AssignmentMemberDto>();
}

public class AssignmentMemberDto
{
    public int MemberId { get; set; }
    public string Name { get; set; }
    public string TeamRule { get; set; }
}