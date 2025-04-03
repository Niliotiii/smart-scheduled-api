using SmartScheduledApi.Dtos;

public interface IMemberService
{
    Task<MemberInfo?> GetMemberInfoAsync(int userId, int teamId);
}