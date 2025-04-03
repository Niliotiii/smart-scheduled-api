namespace SmartScheduledApi.Enums;

[Flags]
public enum Permission
{
    None = 0,
    ViewTeam = 1 << 0,
    EditTeam = 1 << 1,
    DeleteTeam = 1 << 2,
    ManageMembers = 1 << 3,
    CreateSchedule = 1 << 4,
    EditSchedule = 1 << 5,
    DeleteSchedule = 1 << 6,
    AssignMembers = 1 << 7,
    ViewSchedule = 1 << 8,
    All = ~None
}
