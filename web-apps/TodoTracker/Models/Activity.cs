namespace TodoTracker.Models;

/// <summary>
/// Represents an activity within a project, derived from the second "#tag" in task titles.
/// </summary>
public class Activity
{
    public string Name { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public List<TodoItem> Tasks { get; set; } = new();
}
