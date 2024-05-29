using Microsoft.AspNetCore.Mvc;
using TestPlatform.Domain;
using TestPlatform.Domain.Entity;
using TestPlatform.Models.TestModels;
using TestPlatform.Services.Test;

namespace TestPlatform.Controllers;

public class TestController : Controller
{
    private readonly TestPlatrormDbContext _dbContext;
    private readonly ITestService _testService;

    public TestController(TestPlatrormDbContext dbContext, ITestService testService)
    {
        _dbContext = dbContext;
        _testService = testService;
    }

    public IActionResult StartTest([FromRoute] string id)
    {
        String studentId;
        try
        {
            studentId = HttpContext.Session.GetString("authUserId");
        }
        catch
        {
            return RedirectToAction("Auth", "Account");
        }

        String testId;
        try
        {
            testId = id;
        }
        catch
        {
            return RedirectToAction("Main", "Home");
        }

        if (_dbContext.Journals.Any(j => j.UserId == studentId && j.TestId == testId))
        {
            // уже открыт тест
        }
        else
        {
            _dbContext.Journals.Add(new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = studentId,
                TestId = testId,
                IsPassed = false,
            });
        }

        List<QuestionModel> questions = _dbContext.Questions
            .Where(q => q.TestId == testId)
            .Select(qs => new QuestionModel()
            {
                Id = qs.Id,
                TestId = qs.TestId.ToString(),
                Text = qs.Text,
                Answers = _dbContext.Answers
                    .Where(a => a.QuestionId == qs.Id)
                    .Select(a => new AnswerModel()
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        Text = a.Text,
                        IsRight = a.IsRight
                    })
                    .ToList()
            })
            .ToList();
        HttpContext.Session.SetString("questions", _testService.SerializeQuestions(questions));
        ViewBag.TotalQuestions = questions.Count;
        return View("DisplayTest");
    }

    public IActionResult DisplayTest([FromRoute] String id)
    {
        String studentId;
        try
        {
            studentId = HttpContext.Session.GetString("authUserId");
        }
        catch
        {
            return RedirectToAction("Auth", "Account");
        }

        String testId;
        try
        {
            testId = id;
        }
        catch
        {
            return RedirectToAction("Main", "Home");
        }

        // проверка существует ли уже начатый тест исходя из журнала
        if (_dbContext.Journals.Any(j => j.UserId == studentId && j.TestId == testId))
        {
            // уже открыт тест
        }
        else
        {
            _dbContext.Journals.Add(new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = studentId,
                TestId = testId,
                IsPassed = false
            });
        }

        List<QuestionModel> questions = _dbContext.Questions
            .Where(q => q.TestId == testId)
            .Select(qs => new QuestionModel()
            {
                Id = qs.Id,
                TestId = qs.TestId.ToString(),
                Text = qs.Text,
                Answers = _dbContext.Answers
                    .Where(a => a.QuestionId == qs.Id)
                    .Select(a => new AnswerModel()
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        Text = a.Text,
                        IsRight = a.IsRight
                    })
                    .ToList(),
                CheckedAnswer = null!,
                Index = 0
            })
            .ToList();
        HttpContext.Session.SetString("questions", _testService.SerializeQuestions(questions));
        ViewBag.TotalQuestions = questions.Count;
        /////////////////////////////////////////////////
        var currentQuestion = questions.First();
        ViewBag.CurrentQuestion = currentQuestion;
        ViewBag.QuestionIndex = 0;
        _dbContext.SaveChanges();
        return View(currentQuestion);
    }

    public IActionResult NextQuestion(int questionIndex)
    {
        var questions = _testService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
        ViewBag.TotalQuestions = questions.Count;
        var nextQuestion = questions[questionIndex];
        ViewBag.CurrentQuestion = nextQuestion;
        ViewBag.QuestionIndex = questionIndex;
        return View("DisplayTest", nextQuestion);
    }

    public IActionResult PreviousQuestion(int questionIndex)
    {
        var questions = _testService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
        ViewBag.TotalQuestions = questions.Count;
        var prevQuestion = questions[questionIndex];
        ViewBag.CurrentQuestion = prevQuestion;
        ViewBag.QuestionIndex = questionIndex;
        return View("DisplayTest", prevQuestion);
    }

    public IActionResult UnifiedPagination(QuestionModel model, string button, int questionIndex)
    {
        if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
        {
            var questions = _testService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
            ViewBag.TotalQuestions = questions.Count;
            String selectedItem;
            try
            {
                selectedItem = model.CheckedAnswer;
            }
            catch
            {
                selectedItem = null;
            }


            var question = questions[questionIndex];
            var answer = _dbContext.Answers.FirstOrDefault(a => a.Id == selectedItem);
            if (question is not null && answer is not null)
            {
                var studentId = HttpContext.Session.GetString("authUserId");

                StudentAnswers studentAnswer = null!;
                try
                {
                    studentAnswer = _dbContext.StudentAnswers.FirstOrDefault(sa =>
                        sa.UserId == HttpContext.Session.GetString("authUserId") &&
                        sa.TestId == question.TestId &&
                        sa.QuestionId == question.Id);
                }
                catch
                {
                }

                if (studentAnswer is not null)
                {
                    _dbContext.StudentAnswers.Remove(studentAnswer);
                }

                _dbContext.StudentAnswers.Add(new()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = studentId,
                    TestId = question.TestId,
                    QuestionId = question.Id,
                    AnswerId = answer.Id,
                    IsRight = answer.IsRight,
                });
            }

            switch (button)
            {
                case "previous":
                {
                    var prevQuestion = questions[questionIndex - 1];
                    ViewBag.CurrentQuestion = prevQuestion;
                    ViewBag.QuestionIndex = questionIndex - 1;
                    _dbContext.SaveChanges();
                    return View("DisplayTest", prevQuestion);
                }
                case "next":
                {
                    var nextQuestion = questions[questionIndex + 1];
                    ViewBag.CurrentQuestion = nextQuestion;
                    ViewBag.QuestionIndex = questionIndex + 1;
                    _dbContext.SaveChanges();
                    return View("DisplayTest", nextQuestion);
                }
                case "finish":
                    var testId = question.TestId;
                    var studentId = HttpContext.Session.GetString("authUserId");
                    var journal =
                        _dbContext.Journals.FirstOrDefault(j => j.TestId == testId && j.UserId == studentId);
                    if (journal is not null)
                    {
                        var studentAnswers = _dbContext.StudentAnswers
                            .Where(sa => sa.UserId == studentId && sa.TestId == testId)
                            .ToList();
                        var totalQuestions = _dbContext.Questions.Count(q => q.TestId == testId);

                        var totalRightAnswers = studentAnswers.Count(sa => sa.IsRight);

                        var result = 0;
                        if (studentAnswers != null && studentAnswers.Count > 0)
                        {
                            result = (int)Math.Round((double)totalRightAnswers / totalQuestions * 100);
                        }

                        if (totalRightAnswers > 0 && studentAnswers.Count > 0 && result >= 60)
                        {
                            journal.Result = $"{result}%";
                            journal.IsPassed = true;
                        }
                        else
                        {
                            journal.Result = $"{result}%";
                            journal.IsPassed = false;
                        }

                        var assignedTest = _dbContext.AssignedTests.FirstOrDefault(sa => sa.StudentId == studentId && sa.TestId == testId);
                        if (assignedTest != null)
                        {
                            assignedTest.IsPassed = true;
                            _dbContext.Update(assignedTest);
                        }

                        _dbContext.SaveChanges();
                        return View("TestResult", new TestResultModel()
                        {
                            Result = journal.Result,
                            IsPassed = journal.IsPassed
                        });
                    }

                    break;
                default:
                    throw new Exception("Invalid button");
            }

            return NotFound();
        }

        return RedirectToAction("Auth", "Account");
    }
}