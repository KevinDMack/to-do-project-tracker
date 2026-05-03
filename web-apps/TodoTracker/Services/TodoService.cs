using System.Text.RegularExpressions;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TodoTracker.Models;

namespace TodoTracker.Services;

/// <summary>
/// Fetches Microsoft To-Do tasks via the Graph SDK and organises them by "#project" and "#activity" tags.
/// </summary>
public class TodoService : ITodoService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<TodoService> _logger;

    // Matches one or more "#word" tokens anywhere in the task title.
    private static readonly Regex TagPattern = new(@"#(\w+)", RegexOptions.Compiled);

    public TodoService(GraphServiceClient graphClient, ILogger<TodoService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetProjectsAsync()
    {
        var projects = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // Fetch all To-Do task lists for the signed-in user.
            var listsResponse = await _graphClient.Me.Todo.Lists.GetAsync();
            if (listsResponse?.Value == null) return new List<Project>();

            foreach (var list in listsResponse.Value)
            {
                if (string.IsNullOrEmpty(list.Id)) continue;

                var tasksResponse = await _graphClient.Me.Todo.Lists[list.Id].Tasks.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Filter = "status ne 'completed'";
                });

                if (tasksResponse?.Value == null) continue;

                foreach (var graphTask in tasksResponse.Value)
                {
                    var item = MapToTodoItem(graphTask);
                    if (string.IsNullOrWhiteSpace(item.ProjectTag)) continue;

                    if (!projects.TryGetValue(item.ProjectTag, out var project))
                    {
                        project = new Project { Name = item.ProjectTag };
                        projects[item.ProjectTag] = project;
                    }

                    var activityName = string.IsNullOrWhiteSpace(item.ActivityTag) ? "General" : item.ActivityTag;
                    var activity = project.Activities.FirstOrDefault(a =>
                        string.Equals(a.Name, activityName, StringComparison.OrdinalIgnoreCase));

                    if (activity == null)
                    {
                        activity = new Activity { Name = activityName, ProjectName = item.ProjectTag };
                        project.Activities.Add(activity);
                    }

                    activity.Tasks.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching To-Do tasks from Microsoft Graph");
        }

        return projects.Values.OrderBy(p => p.Name).ToList();
    }

    /// <inheritdoc/>
    public async Task<TodoItem> CreateTaskAsync(CreateTaskRequest request)
    {
        // Build the canonical title: #Project #Activity <user title>
        var fullTitle = $"#{request.ProjectName} #{request.ActivityName} {request.Title}".Trim();

        // Find (or create) the default task list.
        var listsResponse = await _graphClient.Me.Todo.Lists.GetAsync(cfg =>
        {
            cfg.QueryParameters.Filter = "wellknownListName eq 'defaultList'";
        });

        var listId = listsResponse?.Value?.FirstOrDefault()?.Id;
        if (string.IsNullOrEmpty(listId))
        {
            // Fall back to the first available list.
            var all = await _graphClient.Me.Todo.Lists.GetAsync();
            listId = all?.Value?.FirstOrDefault()?.Id
                     ?? throw new InvalidOperationException("No To-Do list found for the user.");
        }

        var newTask = new TodoTask
        {
            Title = fullTitle,
            Body = new ItemBody { Content = request.Notes ?? string.Empty, ContentType = BodyType.Text }
        };

        if (request.DueDate.HasValue)
        {
            newTask.DueDateTime = new DateTimeTimeZone
            {
                DateTime = request.DueDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = "UTC"
            };
        }

        var created = await _graphClient.Me.Todo.Lists[listId].Tasks.PostAsync(newTask)
                      ?? throw new InvalidOperationException("Graph API returned null when creating the task.");

        return MapToTodoItem(created);
    }

    // ----- private helpers -----

    private static TodoItem MapToTodoItem(TodoTask task)
    {
        var title = task.Title ?? string.Empty;
        var tags = TagPattern.Matches(title).Select(m => m.Groups[1].Value).ToList();

        var projectTag = tags.Count > 0 ? tags[0] : string.Empty;
        var activityTag = tags.Count > 1 ? tags[1] : string.Empty;
        var cleanTitle = TagPattern.Replace(title, string.Empty).Trim();

        return new TodoItem
        {
            Id = task.Id ?? string.Empty,
            Title = title,
            CleanTitle = cleanTitle,
            ProjectTag = projectTag,
            ActivityTag = activityTag,
            IsCompleted = task.Status == Microsoft.Graph.Models.TaskStatus.Completed,
            DueDate = task.DueDateTime != null && DateTime.TryParse(task.DueDateTime.DateTime, out var dueDate)
                ? dueDate
                : null,
            Notes = task.Body?.Content
        };
    }
}
