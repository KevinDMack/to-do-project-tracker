namespace TodoTracker.Models;

/// <summary>
/// Payload sent from the "Add Task" modal dialog.
/// </summary>
public class CreateTaskRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}
