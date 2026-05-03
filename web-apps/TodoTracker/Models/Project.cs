namespace TodoTracker.Models;

/// <summary>
/// Represents a project/initiative derived from the first "#tag" in task titles.
/// </summary>
public class Project
{
    public string Name { get; set; } = string.Empty;
    public List<Activity> Activities { get; set; } = new();
}
