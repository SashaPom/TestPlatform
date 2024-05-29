using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestPlatform.Domain;
using TestPlatform.Models;
using TestPlatform.Models.TestModels;

namespace TestPlatform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TestPlatrormDbContext _dbContext;
    public HomeController(ILogger<HomeController> logger, TestPlatrormDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Main()
    {
        var userIdString = HttpContext.Session.GetString("authUserId");
        try
        {
            var userId = userIdString;
            var assignedTests = _dbContext.AssignedTests.Where(at => at.StudentId == userId).ToList();
            var tests = _dbContext.Tests.ToList();
            var filteredTests = tests.Where(t => assignedTests.Any(at => at.TestId == t.Id && !at.IsPassed)).ToList();
            MainViewModel model = new()
            {
                Tests = filteredTests.Select(t => new TestModel
                {
                    Id = t.Id,
                    MentorId = t.MentorId,
                    Icon = t.Icon,
                    Name = t.Name,
                    Description = t.Description,
                    MentorName = _dbContext.Users.FirstOrDefault(u => u.Id == t.MentorId).RealName ?? String.Empty,
                    QuestionsCount = _dbContext.Questions.Count(q => q.TestId == t.Id)
                }).ToList()
            };
            return View(model);
        }
        catch (Exception)
        {
            return RedirectToAction("Auth", "Account");
        }

        return NotFound();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statuscode)
    {
        if (statuscode == 404)
        {
            return View("404");
        }
        else
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}




