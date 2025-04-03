using System.Threading.Tasks;

public interface IMemberService
{
    Task<MemberInfo?> GetMemberInfoAsync(int userId, int teamId);
}