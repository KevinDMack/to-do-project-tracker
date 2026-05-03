namespace TodoTracker.Models;

/// <summary>
/// View-model used on the project detail page.
/// </summary>
public class ProjectViewModel
{
    public List<Project> AllProjects { get; set; } = new();
    public Project? SelectedProject { get; set; }
}
