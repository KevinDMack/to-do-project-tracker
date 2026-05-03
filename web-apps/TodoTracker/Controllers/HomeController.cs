using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoTracker.Models;
using TodoTracker.Services;

namespace TodoTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITodoService _todoService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITodoService todoService, ILogger<HomeController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    /// <summary>Lists all projects in the left-hand panel; shows the first project's activities by default.</summary>
    public async Task<IActionResult> Index(string? project)
    {
        var allProjects = await _todoService.GetProjectsAsync();

        var selected = string.IsNullOrWhiteSpace(project)
            ? allProjects.FirstOrDefault()
            : allProjects.FirstOrDefault(p => string.Equals(p.Name, project, StringComparison.OrdinalIgnoreCase));

        var vm = new ProjectViewModel
        {
            AllProjects = allProjects,
            SelectedProject = selected
        };

        return View(vm);
    }

    public IActionResult Error()
    {
        return View();
    }
}
