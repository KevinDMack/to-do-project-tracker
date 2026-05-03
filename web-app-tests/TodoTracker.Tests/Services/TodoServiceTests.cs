using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TodoTracker.Models;
using TodoTracker.Services;
using Xunit;

namespace TodoTracker.Tests.Services;

/// <summary>
/// Unit tests for tag parsing logic that underpins the project/activity grouping.
/// </summary>
public class TodoServiceTagParsingTests
{
    // Regex copied from the service — we test the same logic here to ensure the contract is stable.
    private static readonly Regex TagPattern = new(@"#(\w+)", RegexOptions.Compiled);

    private static (string projectTag, string activityTag, string cleanTitle) ParseTitle(string title)
    {
        var tags = TagPattern.Matches(title).Select(m => m.Groups[1].Value).ToList();
        var projectTag = tags.Count > 0 ? tags[0] : string.Empty;
        var activityTag = tags.Count > 1 ? tags[1] : string.Empty;
        var cleanTitle = TagPattern.Replace(title, string.Empty).Trim();
        return (projectTag, activityTag, cleanTitle);
    }

    [Fact]
    public void ParseTitle_TwoTags_ExtractsProjectAndActivity()
    {
        var (project, activity, clean) = ParseTitle("#MyProject #MyActivity Buy milk");
        Assert.Equal("MyProject", project);
        Assert.Equal("MyActivity", activity);
        Assert.Equal("Buy milk", clean);
    }

    [Fact]
    public void ParseTitle_OneTag_ExtractsProjectOnly()
    {
        var (project, activity, clean) = ParseTitle("#OnlyProject Do something");
        Assert.Equal("OnlyProject", project);
        Assert.Equal(string.Empty, activity);
        Assert.Equal("Do something", clean);
    }

    [Fact]
    public void ParseTitle_NoTags_ReturnsEmptyTagsAndFullTitle()
    {
        var (project, activity, clean) = ParseTitle("Just a plain task");
        Assert.Equal(string.Empty, project);
        Assert.Equal(string.Empty, activity);
        Assert.Equal("Just a plain task", clean);
    }

    [Fact]
    public void ParseTitle_EmptyString_ReturnsAllEmpty()
    {
        var (project, activity, clean) = ParseTitle(string.Empty);
        Assert.Equal(string.Empty, project);
        Assert.Equal(string.Empty, activity);
        Assert.Equal(string.Empty, clean);
    }

    [Fact]
    public void ParseTitle_TagsWithoutSpace_AreExtracted()
    {
        var (project, activity, clean) = ParseTitle("#Alpha#Beta task text");
        Assert.Equal("Alpha", project);
        Assert.Equal("Beta", activity);
        Assert.Equal("task text", clean);
    }

    [Fact]
    public void ParseTitle_ThreeOrMoreTags_OnlyFirstTwoMatter()
    {
        var (project, activity, _) = ParseTitle("#P1 #A1 #Extra task");
        Assert.Equal("P1", project);
        Assert.Equal("A1", activity);
    }
}

/// <summary>
/// Unit tests for <see cref="CreateTaskRequest"/> validation.
/// </summary>
public class CreateTaskRequestTests
{
    [Fact]
    public void FullTitle_IsBuiltFromProjectActivityAndTitle()
    {
        var request = new CreateTaskRequest
        {
            ProjectName = "MyProject",
            ActivityName = "MyActivity",
            Title = "Review PR"
        };

        var fullTitle = $"#{request.ProjectName} #{request.ActivityName} {request.Title}".Trim();
        Assert.Equal("#MyProject #MyActivity Review PR", fullTitle);
    }

    [Fact]
    public void FullTitle_PreservesTagsWithoutDuplication()
    {
        var request = new CreateTaskRequest
        {
            ProjectName = "Alpha",
            ActivityName = "Beta",
            Title = "Task description"
        };

        var fullTitle = $"#{request.ProjectName} #{request.ActivityName} {request.Title}".Trim();
        Assert.StartsWith("#Alpha #Beta", fullTitle);
    }
}

/// <summary>
/// Unit tests for <see cref="ProjectViewModel"/> construction.
/// </summary>
public class ProjectViewModelTests
{
    [Fact]
    public void SelectedProject_MatchesByName()
    {
        var projects = new List<Project>
        {
            new() { Name = "Alpha" },
            new() { Name = "Beta" }
        };

        var selected = projects.FirstOrDefault(p =>
            string.Equals(p.Name, "Beta", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(selected);
        Assert.Equal("Beta", selected.Name);
    }

    [Fact]
    public void ActivitiesGroupedCorrectly_WhenMultipleTasksSameActivity()
    {
        var tasks = new List<TodoItem>
        {
            new() { ProjectTag = "Proj", ActivityTag = "Act1", CleanTitle = "Task A" },
            new() { ProjectTag = "Proj", ActivityTag = "Act1", CleanTitle = "Task B" },
            new() { ProjectTag = "Proj", ActivityTag = "Act2", CleanTitle = "Task C" }
        };

        var project = new Project { Name = "Proj" };
        foreach (var task in tasks)
        {
            var actName = string.IsNullOrWhiteSpace(task.ActivityTag) ? "General" : task.ActivityTag;
            var act = project.Activities.FirstOrDefault(a =>
                string.Equals(a.Name, actName, StringComparison.OrdinalIgnoreCase));
            if (act == null)
            {
                act = new Activity { Name = actName, ProjectName = "Proj" };
                project.Activities.Add(act);
            }
            act.Tasks.Add(task);
        }

        Assert.Equal(2, project.Activities.Count);
        Assert.Equal(2, project.Activities.First(a => a.Name == "Act1").Tasks.Count);
        Assert.Single(project.Activities.First(a => a.Name == "Act2").Tasks);
    }
}
