using TodoTracker.Models;

namespace TodoTracker.Services;

/// <summary>
/// Abstraction over the Microsoft To-Do Graph API operations used by this application.
/// </summary>
public interface ITodoService
{
    /// <summary>Retrieves all To-Do tasks for the signed-in user and organises them into projects.</summary>
    Task<List<Project>> GetProjectsAsync();

    /// <summary>Creates a new To-Do task pre-populated with the supplied project and activity tags.</summary>
    Task<TodoItem> CreateTaskAsync(CreateTaskRequest request);
}
