namespace TodoTracker.Models;

/// <summary>
/// Represents a single Microsoft To-Do task enriched with parsed tag information.
/// </summary>
public class TodoItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }

    /// <summary>First "#tag" in the task title — used as the Project/Initiative label.</summary>
    public string ProjectTag { get; set; } = string.Empty;

    /// <summary>Second "#tag" in the task title — used as the Activity label within the project.</summary>
    public string ActivityTag { get; set; } = string.Empty;

    /// <summary>The remaining title text after the tags have been stripped.</summary>
    public string CleanTitle { get; set; } = string.Empty;
}
