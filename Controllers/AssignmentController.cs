using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduledApi.DataContext;
using SmartScheduledApi.Models;
using SmartScheduledApi.Dtos;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SmartScheduledApi.Services;
using SmartScheduledApi.Enums;

namespace SmartScheduledApi.Controllers;

[ApiController]
[Route("api/teams/{teamId}/[controller]")]
[Authorize]
public class AssignmentController : BaseController
{
    private readonly SmartScheduleApiContext _context;
    private readonly UserContextService _userContext;

    public AssignmentController(
        SmartScheduleApiContext context,
        UserContextService userContext,
        IPermissionService permissionService) : base(permissionService)
    {
        _context = context;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments(int teamId)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewSchedules))
            return Forbidden("You don't have permission to view assignments");

        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        // Buscar assignments direto da tabela Assignment por teamId
        var assignments = await _context.Assignments
            .Where(a => a.TeamId == teamId)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                TeamId = a.TeamId,
                TeamName = a.Team.Name
            })
            .ToListAsync();

        return ApiResponse(assignments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignment(int teamId, [FromBody] CreateAssignmentDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.CreateSchedules))
            return Forbidden("You don't have permission to create assignments");

        if (!ModelState.IsValid)
            return InvalidRequest("Invalid assignment data", ModelState);

        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            return NotFound("Team not found");

        try
        {
            // Criar assignment associado ao time
            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                TeamId = teamId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Assignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            // Não precisamos buscar o time novamente, já temos a variável 'team'

            var response = new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CreatedAt = assignment.CreatedAt,
                TeamId = assignment.TeamId,
                TeamName = team.Name  // Use a variável team que já temos
            };

            return Created(response);
        }
        catch (Exception ex)
        {
            return ApiError("Error creating assignment", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    // Novo endpoint para listar os membros associados a um Assignment
    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetAssignmentMembers(int teamId, int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewAssignments))
            return Forbidden("You don't have permission to view assignments");

        var assignment = await _context.Assignments
            .Where(a => a.Id == id && a.TeamId == teamId)
            .FirstOrDefaultAsync();

        if (assignment == null)
            return NotFound("Assignment not found");

        var members = await _context.Assigneds
            .Where(a => a.AssignmentId == id)
            .Include(a => a.Member)
                .ThenInclude(m => m.User)
            .Select(a => new AssignmentMemberDto
            {
                MemberId = a.Member.Id,
                Name = a.Member.User.Name,
                TeamRule = a.Member.TeamRule.ToString()
            })
            .ToListAsync();

        var response = new AssignmentMembersDto
        {
            Id = assignment.Id,
            Title = assignment.Title,
            Description = assignment.Description,
            Members = members
        };

        return ApiResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssignment(int teamId, int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.ViewSchedules))
            return Forbidden("You don't have permission to view assignments");

        // Buscar assignment diretamente pelo teamId e id
        var assignment = await _context.Assignments
            .Include(a => a.Team)
            .FirstOrDefaultAsync(a => a.Id == id && a.TeamId == teamId);

        if (assignment == null)
            return NotFound("Assignment not found");

        // Buscar os membros associados a este assignment via Assigned
        var members = await _context.Assigneds
            .Where(a => a.AssignmentId == id)
            .Include(a => a.Member)
                .ThenInclude(m => m.User)
            .Select(a => new AssignmentMemberDto
            {
                MemberId = a.Member.Id,
                Name = a.Member.User.Name,
                TeamRule = a.Member.TeamRule.ToString()
            })
            .ToListAsync();

        var response = new AssignmentDetailResponseDto
        {
            Id = assignment.Id,
            Title = assignment.Title,
            Description = assignment.Description,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt,
            TeamId = assignment.TeamId,
            TeamName = assignment.Team.Name,
            Members = members
        };

        return ApiResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssignment(int teamId, int id, [FromBody] UpdateAssignmentDto dto)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.EditSchedules))
            return Forbidden("You don't have permission to edit assignments");

        // Buscar assignment pelo Assigned.Member
        var assignedWithAssignment = await _context.Assigneds
            .Where(a => a.Assignment.Id == id && a.Member.TeamId == teamId)
            .Include(a => a.Assignment)
            .FirstOrDefaultAsync();

        if (assignedWithAssignment == null)
            return NotFound("Assignment not found");

        try
        {
            assignedWithAssignment.Assignment.Title = dto.Title;
            assignedWithAssignment.Assignment.Description = dto.Description;
            assignedWithAssignment.Assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse(assignedWithAssignment.Assignment, "Assignment updated successfully");
        }
        catch (Exception ex)
        {
            return ApiError("Error updating assignment", HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssignment(int teamId, int id)
    {
        var userId = _userContext.GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!await EnsureTeamPermissionAsync(userId.Value, teamId, TeamPermission.DeleteSchedules))
            return Forbidden("You don't have permission to delete assignments");

        // Buscar assignment pelo Assigned.Member
        var assignedWithAssignment = await _context.Assigneds
            .Where(a => a.Assignment.Id == id && a.Member.TeamId == teamId)
            .Include(a => a.Assignment)
            .FirstOrDefaultAsync();

        if (assignedWithAssignment == null)
            return NotFound("Assignment not found");

        // Usar uma transação para deletar primeiro os relacionamentos e depois o assignment
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Deletar todos os assigned relacionados a esse assignment
                var assigneds = await _context.Assigneds
                    .Where(a => a.AssignmentId == id)
                    .ToListAsync();

                _context.Assigneds.RemoveRange(assigneds);
                await _context.SaveChangesAsync();

                // Deletar o assignment
                _context.Assignments.Remove(assignedWithAssignment.Assignment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ApiResponse(assigneds, "Assignment deleted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiError("Error deleting assignment", HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
