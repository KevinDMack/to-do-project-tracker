using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoTracker.Models;
using TodoTracker.Services;

namespace TodoTracker.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    /// <summary>Creates a new To-Do item via the "Add" modal dialog.</summary>
    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required." });

        if (string.IsNullOrWhiteSpace(request.ProjectName))
            return BadRequest(new { error = "ProjectName is required." });

        if (string.IsNullOrWhiteSpace(request.ActivityName))
            return BadRequest(new { error = "ActivityName is required." });

        try
        {
            var created = await _todoService.CreateTaskAsync(request);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create task for project={Project} activity={Activity}",
                request.ProjectName.Replace(Environment.NewLine, ""), request.ActivityName.Replace(Environment.NewLine, ""));
            return StatusCode(500, new { error = "An error occurred while creating the task." });
        }
    }
}
