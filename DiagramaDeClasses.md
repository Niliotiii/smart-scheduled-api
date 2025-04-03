# Diagrama de Classes - Smart Scheduled API

```mermaid
classDiagram
    class User {
        +int Id
        +string Username
        +string Password
        +string Email
        +List~UserTeam~ Teams
        +List~Role~ Roles
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsActive
        +Login()
        +SelectTeam()
    }

    class Role {
        +int Id
        +string Name
        +string Description
        +List~Permission~ Permissions
    }

    class Permission {
        +int Id
        +string Name
        +string Description
    }

    class Team {
        +int Id
        +string Name
        +string Description
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsActive
        +List~User~ Members
    }

    class UserTeam {
        +int UserId
        +int TeamId
        +string Rule
        +DateTime JoinedAt
        +bool IsActive
    }

    class Rule {
        <<enumeration>>
        Leader
        Editor
        Viewer
    }

    class Schedule {
        +int Id
        +int TeamId
        +string Title
        +string Description
        +DateTime StartTime
        +DateTime EndTime
        +int CreatedBy
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsActive
        +Create()
        +Update()
        +Delete()
        +GetById()
        +GetByTeam()
    }

    class RenderController {
        +GetRenderInfo()
        -GetRenderInfoBasedOnRoleAndRule(string role, string rule, string team)
    }

    class AuthController {
        +Login(LoginDto login)
        +Logout()
        +RefreshToken()
    }

    class TeamController {
        +GetAll()
        +GetById(int id)
        +Create(TeamDto team)
        +Update(int id, TeamDto team)
        +Delete(int id)
        +SelectTeam(int teamId)
    }

    class ScheduleController {
        +GetAll()
        +GetById(int id)
        +GetByTeam(int teamId)
        +Create(ScheduleDto schedule)
        +Update(int id, ScheduleDto schedule)
        +Delete(int id)
    }

    class UserService {
        +GetUserById(int id)
        +GetUserByUsername(string username)
        +CreateUser(UserDto user)
        +UpdateUser(int id, UserDto user)
        +DeleteUser(int id)
        +AuthenticateUser(string username, string password)
        +AssignRoleToUser(int userId, int roleId)
        +GetUserPermissions(int userId)
    }

    class TeamService {
        +GetTeamById(int id)
        +GetAllTeams()
        +CreateTeam(TeamDto team)
        +UpdateTeam(int id, TeamDto team)
        +DeleteTeam(int id)
        +AddUserToTeam(int userId, int teamId, string rule)
        +RemoveUserFromTeam(int userId, int teamId)
        +GetTeamMembers(int teamId)
    }

    class ScheduleService {
        +GetScheduleById(int id)
        +GetSchedulesByTeam(int teamId)
        +CreateSchedule(ScheduleDto schedule)
        +UpdateSchedule(int id, ScheduleDto schedule)
        +DeleteSchedule(int id)
    }

    class RenderService {
        +GetRenderInfoForUser(int userId, int teamId)
        +GetUserInterface(string role, string rule, string team)
    }

    User "1" -- "*" UserTeam : has
    Team "1" -- "*" UserTeam : contains
    UserTeam -- Rule : applies
    User "1" -- "*" Role : has
    Role "1" -- "*" Permission : contains
    Team "1" -- "*" Schedule : has
    User "1" -- "*" Schedule : creates

    RenderController --> RenderService : uses
    AuthController --> UserService : uses
    TeamController --> TeamService : uses
    ScheduleController --> ScheduleService : uses

    UserService --> User : manages
    TeamService --> Team : manages
    ScheduleService --> Schedule : manages
    RenderService --> User : checks
    RenderService --> Team : checks
    RenderService --> Rule : evaluates
```

## Descrição das Classes Principais

### Entidades

1. **User**: Representa um usuário do sistema.
2. **Role**: Representa um papel/função no sistema (ex: Admin, User).
3. **Permission**: Representa uma permissão específica.
4. **Team**: Representa uma equipe/time.
5. **UserTeam**: Associação entre User e Team, com a regra (rule) específica para o usuário naquele time.
6. **Rule**: Enumeração que define os tipos de regras disponíveis para usuários em times.
   - **Leader**: Papel de liderança no time, com permissões completas de gerenciamento.
   - **Editor**: Papel de editor, com permissões para criar e modificar agendamentos.
   - **Viewer**: Papel de visualizador, com permissões somente para leitura.
7. **Schedule**: Representa um agendamento.

### Controladores

1. **RenderController**: Gerencia a renderização baseada em papéis e regras.
2. **AuthController**: Gerencia autenticação de usuários.
3. **TeamController**: Gerencia operações de equipes.
4. **ScheduleController**: Gerencia operações de agendamentos.

### Serviços

1. **UserService**: Encapsula a lógica de negócios relacionada a usuários.
2. **TeamService**: Encapsula a lógica de negócios relacionada a equipes.
3. **ScheduleService**: Encapsula a lógica de negócios relacionada a agendamentos.
4. **RenderService**: Encapsula a lógica de determinação da interface a ser renderizada.

### Relacionamentos

- Um usuário pode pertencer a vários times (UserTeam)
- Um time pode conter vários usuários (UserTeam)
- Um usuário pode ter vários papéis
- Um papel pode ter várias permissões
- Um time pode ter vários agendamentos
- Um usuário pode criar vários agendamentos
- Uma associação UserTeam aplica uma regra específica (Leader, Editor, Viewer)
