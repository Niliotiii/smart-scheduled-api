namespace SmartScheduledApi.Enums
{
    [Flags]
    public enum TeamPermission
    {
        None = 0,
        ViewTeam = 1 << 0,           // Leader, Editor, Viewer: Ver informações do time
        EditTeam = 1 << 1,           // Leader, Editor: Editar informações do time
        CreateAssignments = 1 << 2,   // Leader, Editor: Criar assignments
        EditAssignments = 1 << 3,     // Leader, Editor: Editar assignments
        ViewAssignments = 1 << 4,     // Leader, Editor, Viewer: Ver assignments
        ManageInvites = 1 << 5,       // Leader: Gerenciar convites
        CreateSchedules = 1 << 6,     // Leader, Editor: Criar schedules
        EditSchedules = 1 << 7,       // Leader, Editor: Editar schedules
        DeleteSchedules = 1 << 8,     // Leader, Editor: Deletar schedules
        ViewSchedules = 1 << 9,       // Leader, Editor, Viewer: Ver schedules
        ManageTeamSettings = 1 << 10, // Leader: Gerenciar configurações do time
        All = ~None
    }
}
