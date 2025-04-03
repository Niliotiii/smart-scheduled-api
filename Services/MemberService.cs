using SmartScheduledApi.DataContext;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.Dtos;

public class MemberService : IMemberService
{
    private readonly SmartScheduleApiContext _context;

    public MemberService(SmartScheduleApiContext context)
    {
        _context = context;
    }

    public async Task<MemberInfo?> GetMemberInfoAsync(int userId, int teamId)
    {
        return await _context.Members
            .Where(m => m.UserId == userId && m.TeamId == teamId)
            .Select(m => new MemberInfo
            {
                TeamRule = m.TeamRule.ToString(),
                TeamId = m.TeamId
            })
            .FirstOrDefaultAsync();
    }
}

