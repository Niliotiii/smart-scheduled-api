using Microsoft.AspNetCore.Mvc;
using SmartScheduledApi.Enums;
using SmartScheduledApi.Services;
using System.Net;

namespace SmartScheduledApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected readonly IPermissionService _permissionService;

    protected BaseController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected async Task<bool> EnsureApplicationPermissionAsync(int userId, ApplicationPermission permission)
    {
        if (!await _permissionService.HasApplicationPermissionAsync(userId, permission))
        {
            return false;
        }
        return true;
    }

    protected async Task<bool> EnsureTeamPermissionAsync(int userId, int teamId, TeamPermission permission)
    {
        if (!await _permissionService.HasTeamPermissionAsync(userId, teamId, permission))
        {
            return false;
        }
        return true;
    }

    protected IActionResult ApiResponse<T>(T data, string message = "Success", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new
        {
            Success = true,
            Message = message,
            Data = data
        };

        return StatusCode((int)statusCode, response);
    }

    protected IActionResult ApiError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object errors = null)
    {
        var response = new
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        return StatusCode((int)statusCode, response);
    }

    protected IActionResult Created<T>(T data, string message = "Created successfully")
    {
        return ApiResponse(data, message, HttpStatusCode.Created);
    }

    protected IActionResult NotFound(string message = "Resource not found")
    {
        return ApiError(message, HttpStatusCode.NotFound);
    }

    protected IActionResult Forbidden(string message = "Access denied")
    {
        return ApiError(message, HttpStatusCode.Forbidden);
    }

    protected IActionResult InvalidRequest(string message = "Invalid request", object errors = null)
    {
        return ApiError(message, HttpStatusCode.BadRequest, errors);
    }
}
