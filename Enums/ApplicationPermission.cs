namespace SmartScheduledApi.Enums
{
    [Flags]
    public enum ApplicationPermission
    {
        None = 0,
        ViewTeams = 1 << 0,          // Admin: Ver todos os times
        CreateTeams = 1 << 1,        // Admin: Criar times
        EditTeams = 1 << 2,          // Admin: Editar qualquer time
        DeleteTeams = 1 << 3,        // Admin: Deletar qualquer time
        ViewUsers = 1 << 4,          // Admin: Ver todos os usuários
        CreateUsers = 1 << 5,        // Admin: Criar usuários
        EditUsers = 1 << 6,          // Admin: Editar qualquer usuário
        DeleteUsers = 1 << 7,        // Admin: Deletar qualquer usuário
        ViewOwnUser = 1 << 8,        // Admin, User: Ver próprio perfil
        EditOwnUser = 1 << 9,        // Admin, User: Editar próprio perfil
        ManageOwnInvites = 1 << 10,  // Admin, User: Gerenciar próprios convites
        ViewOwnTeams = 1 << 11,      // Admin, User: Ver próprios times
        ManageSystem = 1 << 12,      // Admin: Gerenciar sistema
        All = ~None
    }
}
