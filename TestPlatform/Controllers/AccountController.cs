using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using TestPlatform.Domain.Entity;
using TestPlatform.Models.UserModels;
using TestPlatform.Models;
using TestPlatform.Domain;
using TestPlatform.Services.Validation;
using TestPlatform.Services.Hash;
using TestPlatform.Services.RandomService;
using TestPlatform.Services.Kdf;

namespace TestPlatform.Controllers;

public class AccountController : Controller
{
    private readonly TestPlatrormDbContext _dbContext;
    private readonly IValidationService _validationService;
    private readonly IHashService _hashService;
    private readonly IRandomService _randomService;
    private readonly IKdfService _kdfService;
    private readonly ILogger<AccountController> _logger;


    public AccountController(IValidationService validationService, TestPlatrormDbContext dbContext, IHashService hashService,
        IKdfService kdfService, IRandomService randomService, ILogger<AccountController> logger)
    {
        _validationService = validationService;
        _dbContext = dbContext;
        _hashService = hashService;
        _kdfService = kdfService;
        _randomService = randomService;
        _logger = logger;
    }

    public IActionResult Registration()
    {
        return View();
    }

    public IActionResult Auth()
    {
        ViewData["results"] = TempData["results"];
        return View();
    }

    public IActionResult RegisterUser(UserRegistrationModel userRegistrationModel)
    {
        UserValidationModel validationResults = new();
        bool isModelValid = true;

        String patternEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$";
        String patternRealName = "^[a-zA-Zа-€ј-я]+([-'][a-zA-Zа-€ј-я]+)*$";

        #region Avatar Uploading

        string avatarFilename = null!;
        if (userRegistrationModel.Avatar is not null)
        {
            if (userRegistrationModel.Avatar.Length <= 1000)
            {
                validationResults.AvatarMessage = "Too small file. File must be larger than 1Kb";
                isModelValid = false;
            }
            else
            {
                String ext = Path.GetExtension(userRegistrationModel.Avatar.FileName);
                String hash = (_hashService.Hash(
                    userRegistrationModel.Avatar.FileName + Guid.NewGuid()))[..16];
                avatarFilename = hash + ext;
                string path = "wwwroot/avatars/" + avatarFilename;
                bool isWrongFile = System.IO.File.Exists(path);
                if (!isWrongFile)
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        userRegistrationModel.Avatar.CopyTo(fileStream);
                    }
                }

                ViewData["avatarFilename"] = avatarFilename;
            }
        }

        #endregion

        #region Login valid

        if (String.IsNullOrEmpty(userRegistrationModel.Login))
        {
            validationResults.LoginMessage = "Login is required";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Login, ValidationTerms.Login))
        {
            validationResults.LoginMessage = "Login doesn't match the pattern";
            isModelValid = false;
        }
        else if (_dbContext.Users.Any(u => u.Login.ToLower() == userRegistrationModel.Login))
        {
            validationResults.LoginMessage = $"Login '{userRegistrationModel.Login}' is already taken";
            isModelValid = false;
        }

        #endregion

        #region password valid

        if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.NotEmpty))
        {
            validationResults.PasswordMessage = "Password is required";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.Password))
        {
            validationResults.PasswordMessage = $"Password is too short. Min is 3 symbols";
            isModelValid = false;
        }

        if (!_validationService.Validate(userRegistrationModel.RepeatPassword, ValidationTerms.NotEmpty))
        {
            validationResults.RepeatPasswordMessage = "Repeat password is required";
            isModelValid = false;
        }
        else if (!userRegistrationModel.RepeatPassword.Equals(userRegistrationModel.Password))
        {
            validationResults.RepeatPasswordMessage = "Passwords do not match the pattern";
            isModelValid = false;
        }

        #endregion

        #region Email valid

        if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.NotEmpty))
        {
            validationResults.EmailMessage = "Email is empty";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.Email))
        {
            validationResults.EmailMessage = "Email do not match the pattern";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(userRegistrationModel.Email, patternEmail, RegexOptions.IgnoreCase))
            {
                validationResults.EmailMessage = "Email is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region RealName valid

        if (String.IsNullOrEmpty(userRegistrationModel.RealName))
        {
            validationResults.RealNameMessage = "Real name is required";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(userRegistrationModel.RealName, patternRealName, RegexOptions.IgnoreCase))
            {
                validationResults.RealNameMessage = "Real name is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region IsAgree valid

        if (!userRegistrationModel.IsAgree)
        {
            validationResults.IsAgreeMessage = "You must agree to the terms";
            isModelValid = false;
        }

        #endregion


        if (isModelValid)
        {
            String salt = _randomService.RandomString(8);

            User user = new()
            {
                Id = Guid.NewGuid().ToString(),

                Login = userRegistrationModel.Login,
                PasswordHash = _kdfService.GetDerivedKey(userRegistrationModel.Password, salt),
                PasswordSalt = salt,

                Role = "Student",
                Email = userRegistrationModel.Email,
                EmailCode = _randomService.ConfirmCode(6),
                RealName = userRegistrationModel.RealName,
                Avatar = avatarFilename,
                RegisterDt = DateTime.Now,
                LastEnterDt = null,
            };
            if (String.IsNullOrEmpty(avatarFilename))
            {
                user.Avatar = "studentAvatar.jpeg";
            }
            _dbContext.Users.Add(user);

            // генеруЇмо токен п≥дтвердженн€ пошти
            // var emailConfirmToken = _GenerateEmailToken(user);
            _dbContext.SaveChanges();

            // надсилаЇмо код п≥дтвердженн€ пошти
            // _SendConfirmEmail(user, emailConfirmToken);
            return RedirectToAction("Auth", "Account");
        }
        else
        {
            ViewData["validationResults"] = validationResults;
            return View("Registration");
        }
    }

    public RedirectToActionResult Logout()
    {
        HttpContext.Session.Remove("authUserId");
        return RedirectToAction("Auth", "Account");
    }

    public IActionResult Profile([FromRoute] String id)
    {

        var user = _dbContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {

            ProfileModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dbContext.Users.Count(u => u.Role == "Student")
            };


            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {

                    model.IsPersonal = true;
                }
            }

            return View(model);
        }
        else
        {

            return NotFound();
        }
    }

    [HttpPost]
    public IActionResult EditProfile(ProfileModel model)
    {
        UserValidationModel validationResults = new();
        bool isModelValid = true;

        String patternEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$";
        String patternRealName = "^[a-zA-Zа-€ј-я]+([-'][a-zA-Zа-€ј-я]+)*$";

        var user = _dbContext.Users.FirstOrDefault(u => u.Login == model.Login);
        _logger.LogInformation($"{user.RealName}, {user.Email}, {user.PasswordHash}");

        if (user == null)
        {
            return NotFound();
        }

        #region Email valid

            if (!_validationService.Validate(model.Email, ValidationTerms.NotEmpty))
        {
            validationResults.EmailMessage = "Email is empty";
            isModelValid = false;
        }
        else if (!_validationService.Validate(model.Email, ValidationTerms.Email))
        {
            validationResults.EmailMessage = "Email do not match the pattern";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(model.Email, patternEmail, RegexOptions.IgnoreCase))
            {
                validationResults.EmailMessage = "Email is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region RealName valid

        if (String.IsNullOrEmpty(model.RealName))
        {
            validationResults.RealNameMessage = "Real name is required";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(model.RealName, patternRealName, RegexOptions.IgnoreCase))
            {
                validationResults.RealNameMessage = "Real name is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region oldPasswords valid

        if (!string.IsNullOrEmpty(model.OldPassword))
        {
            string oldPasswordHash = _kdfService.GetDerivedKey(model.OldPassword, user.PasswordSalt);
            if (oldPasswordHash != user.PasswordHash)
            {
                validationResults.OldPasswordMessage = "Old password is wrong";
                isModelValid = false;
            }
        }

        #endregion

        #region newPasswords valid

        if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
        {

            if (!_validationService.Validate(model.NewPassword, ValidationTerms.NotEmpty))
            {
                validationResults.PasswordMessage = "Password is required";
                isModelValid = false;
            }
            else if (!_validationService.Validate(model.NewPassword, ValidationTerms.Password))
            {
                validationResults.PasswordMessage = $"Password is too short. Min is 3 symbols";
                isModelValid = false;
            }

            if (!_validationService.Validate(model.ConfirmNewPassword, ValidationTerms.NotEmpty))
            {
                validationResults.RepeatPasswordMessage = "Repeat password is required";
                isModelValid = false;
            }
            else if (!model.ConfirmNewPassword.Equals(model.NewPassword))
            {
                validationResults.RepeatPasswordMessage = "Passwords do not match";
                isModelValid = false;
            }
        } else if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmNewPassword))
        {
            validationResults.OldPasswordMessage = "Old password is wrong";
            isModelValid = false;
        }

        #endregion


        if (isModelValid)
        {
            String salt = _randomService.RandomString(8);

            user.RealName = model.RealName;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = _kdfService.GetDerivedKey(model.NewPassword, user.PasswordSalt);
            }
            
            _dbContext.Update(user);
            //   _dbContext.Entry(user).State = EntityState.Modified;

            _dbContext.SaveChanges();
            _logger.LogInformation($"User {model.Login}'s profile updated successfully.");
            _logger.LogInformation($"{user.RealName}, {user.Email}, {user.PasswordHash}");

            return RedirectToAction("Profile", new { id = model.Login });
        }
        else
        {
            ViewData["validationResults"] = validationResults;
            return View("Profile", model);
        }
    }

    public IActionResult ProfileStudents([FromRoute] String id)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {
            ProfileStudentsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dbContext.Users
                    .Where(s => s.Role == "Student")
                    .Select(s => new StudentModel()
                {
                    Id = s.Id,
                    Login = s.Login,
                    Realname = s.RealName,
                    Avatar = s.Avatar ?? "no_avatar.png",
                    Role = s.Role,
                    Tests = null!
                }).ToList()
            };

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return View(model);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult ProfileMentors([FromRoute] String id)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {
            ProfileMentorsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Mentors = _dbContext.Users
                    .Where(s => s.Role == "Mentor")
                    .Select(s => new MentorModel()
                {
                    Id = s.Id,
                    Login = s.Login,
                    Realname = s.RealName,
                    Avatar = s.Avatar ?? "no_avatar.png",
                    Role = s.Role,
                }).ToList()
            };

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return View(model);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult ShowStudentTestsJournal([FromRoute] String id)
    {
        var thisUserStringId = HttpContext.Session.GetString("authUserId");
        try
        {
            var thisUser = _dbContext.Users.FirstOrDefault(u => u.Id == thisUserStringId);
            var tests = _dbContext.Tests.ToList();
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            var journals = _dbContext.Journals.Where(j => j.UserId == user.Id).ToList();
            var assignedTests = _dbContext.AssignedTests.Where(at => at.StudentId == user.Id).ToList();
            StudentTestModel model = new()
            {
                Id = thisUser.Id,
                Login = thisUser.Login,
                RealName = thisUser.RealName,
                Avatar = thisUser.Avatar,
                Role = thisUser.Role,
                StudentName = user.RealName,
                Tests = tests
                    .Where(t => journals.Any(j => j.TestId == t.Id) || assignedTests.Any(at => at.TestId == t.Id))
                    .Select(t => new AllTestsModel()
                    {
                        Icon = t.Icon,
                        Id = t.Id,
                        MentorId = t.MentorId,
                        Name = t.Name,
                        IsPassed = journals.Any(j => j.IsPassed && j.TestId == t.Id),
                        MentorName = t.Mentor?.RealName ?? String.Empty,
                        QuestionsCount = _dbContext.Questions.Count(q => q.TestId == t.Id),
                        Result = journals.FirstOrDefault(j => j.TestId == t.Id)?.Result,
                    }).ToList()
            };
            return View("StudentJournal", model);
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult MakeUserMentor([FromRoute] String id)
    {
        var thisUserStringId = HttpContext.Session.GetString("authUserId");
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user.Role == "Student")
            {
                user.Role = "Mentor";
                if(user.Avatar == "studentAvatar.jpeg" || user.Avatar == "no_avatar.png")
                {
                    user.Avatar = "mentorAvatar.png";
                }
                _dbContext.Entry(user).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }

            var thisUser = _dbContext.Users.FirstOrDefault(u => u.Id == thisUserStringId);
            ProfileStudentsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dbContext.Users.Select(s => new StudentModel()
                {
                    Id = s.Id,
                    Login = s.Login,
                    Realname = s.RealName,
                    Avatar = s.Avatar ?? "no_avatar.png",
                    Role = s.Role,
                    Tests = null!
                }).ToList()
            };

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return RedirectToAction("ProfileStudents", "Account", new { id = thisUser.Login });
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult MakeUserStudent([FromRoute] String id)
    {
        var thisUserStringId = HttpContext.Session.GetString("authUserId");
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user.Role == "Mentor")
            {
                user.Role = "Student";
                if (user.Avatar == "mentorAvatar.png" || user.Avatar == "no_avatar.png")
                {
                    user.Avatar = "studentAvatar.jpeg";
                }
                _dbContext.Entry(user).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }

            var thisUser = _dbContext.Users.FirstOrDefault(u => u.Id == thisUserStringId);
            ProfileStudentsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dbContext.Users.Select(s => new StudentModel()
                {
                    Id = s.Id,
                    Login = s.Login,
                    Realname = s.RealName,
                    Avatar = s.Avatar ?? "no_avatar.png",
                    Role = s.Role,
                    Tests = null!
                }).ToList()
            };

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return RedirectToAction("ProfileStudents", "Account", new { id = thisUser.Login });
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult AssignTest([FromRoute] String id)
    {
        var userStringId = HttpContext.Session.GetString("authUserId");
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userStringId);
            var test = _dbContext.Tests.FirstOrDefault(t => t.Id == id);
            // получаем общее количество пользователей в базе данных
            if (user is not null)
            {
                ProfileStudentsModel model = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    TestName = test.Name,
                    Students = _dbContext.Users.Where(s => s.Role == "Student").Select(s => new StudentModel()
                    {
                        Id = s.Id,
                        Login = s.Login,
                        Realname = s.RealName,
                        Avatar = s.Avatar ?? "no_avatar.png",
                        Role = s.Role,
                        AssignTestId = test.Id,
                        IsTestAssigned = _dbContext.AssignedTests.Any(at => at.StudentId == s.Id && at.TestId == test.Id),
                        Tests = null!
                    }).ToList()
                };

                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">user id</param>
    /// <param name="testId">test id</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult AssignTestToStudent(String id, String testId)
    {
        var userStringId = HttpContext.Session.GetString("authUserId");
        try
        {
            var student = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            var test = _dbContext.Tests.FirstOrDefault(t => t.Id == testId);
            var mentor = _dbContext.Users.FirstOrDefault(u => u.Id == userStringId);
            var assignedTests = _dbContext.AssignedTests.Where(at => at.StudentId == student.Id && at.TestId == test.Id).ToList();
            if (student is not null && test is not null && mentor is not null && assignedTests.Count == 0)
            {
                AssignedTest assignedTest = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    StudentId = student.Id,
                    TestId = test.Id,
                    MentorId = mentor.Id,
                    IsPassed = false
                };
                _dbContext.AssignedTests.Add(assignedTest);
                _dbContext.SaveChanges();
                return RedirectToAction("AssignTest", "Account", new { id = test.Id });
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult ProfileTests([FromRoute] String id)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {
            var tests = _dbContext.Tests.ToList();
            var journals = _dbContext.Journals.Where(j => j.UserId == user.Id).ToList();
            var assignedTests = _dbContext.AssignedTests.Where(at => at.StudentId == user.Id && at.IsPassed).ToList();
            StudentTestModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Role = user.Role,
                Tests = tests
                    .Where(t => journals.Any(j => j.TestId == t.Id) || assignedTests.Any(at => at.TestId == t.Id))
                    .Select(t => new AllTestsModel()
                    {
                        Icon = t.Icon,
                        Id = t.Id,
                        MentorId = t.MentorId,
                        Name = t.Name,
                        IsPassed = journals.Any(j => j.IsPassed && j.TestId == t.Id),
                        MentorName = t.Mentor?.RealName ?? String.Empty,
                        QuestionsCount = _dbContext.Questions.Count(q => q.TestId == t.Id),
                        Result = journals.FirstOrDefault(j => j.TestId == t.Id)?.Result,
                    }).ToList()
            };

            return View("ProfileStudentTests", model);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult ProfileTestsEdit([FromRoute] String id)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {

                ProfileTestsModel model = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    Tests = _dbContext.Tests.Where(t => t.MentorId == user.Id).AsEnumerable().Select(t =>
                        new MentorTestModel()
                        {
                            TestIcon = t.Icon,
                            TestDescription = t.Description,
                            Id = t.Id,
                            MentorId = t.MentorId,
                            TestTitle = t.Name,
                            Questions = null!
                        }).ToList()
                };
                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }

                return View("ProfileTests", model);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }


    /// <summary>
    /// Test deleting from database 
    /// </summary>
    /// <param name="id">Test id</param>
    /// <returns></returns>
    public IActionResult DeleteTest(String id)
    {
        var userIdString = HttpContext.Session.GetString("authUserId");
        if (userIdString is null)
        {
            return RedirectToAction("Auth", "Account");
        }

        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userIdString);

            if (user is not null)
            {
                var test = _dbContext.Tests.Include(t => t.Questions).ThenInclude(q => q.Answers)
                    .FirstOrDefault(t => t.Id == id);
                if (test == null)
                {
                    return NotFound();
                }

                // ”даление всех ответов, св€занных с вопросами теста
                foreach (var question in test.Questions)
                {
                    _dbContext.Answers.RemoveRange(question.Answers);
                }

                // ”даление всех вопросов теста
                _dbContext.Questions.RemoveRange(test.Questions);

                // ”даление самого теста
                _dbContext.Tests.Remove(test);

                _dbContext.SaveChanges();

                ProfileTestsModel model = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    Tests = _dbContext.Tests.Where(t => t.MentorId == user.Id).AsEnumerable().Select(t =>
                        new MentorTestModel()
                        {
                            TestIcon = t.Icon,
                            TestDescription = t.Description,
                            Id = t.Id,
                            MentorId = t.MentorId,
                            TestTitle = t.Name,
                            Questions = null!
                        }).ToList()
                };


                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }

                return View("ProfileTests", model);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult EditQuestions([FromRoute] String id)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u =>
                u.Id == HttpContext.Session.GetString("authUserId"));

            if (user is not null)
            {
                var test = _dbContext.Tests.Include(t => t.Questions).ThenInclude(q => q.Answers)
                    .FirstOrDefault(t => t.Id == id);
                if (test == null)
                {
                    return NotFound();
                }

                var questions = _dbContext.Questions
                    .Include(q => q.Answers)
                    .Where(q => q.TestId == id)
                    .ToList();
                ProfileTestsModel model = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    EditingTestId = test.Id,
                    EditingTestName = test.Name,
                    EditingTestDescription = test.Description,
                    EditingTestIcon = test.Icon,
                    Questions = questions
                };


                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult EditTestProps(ProfileTestsModel model)
    {
        var test = _dbContext.Tests.FirstOrDefault(t => t.Id == model.EditingTestId);
        var userId = HttpContext.Session.GetString("authUserId");
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (test is not null)
            {
                if (!String.IsNullOrEmpty(model.EditingTestDescription))
                {
                }

                test.Description = model.EditingTestDescription;
                if (!String.IsNullOrEmpty(model.EditingTestName))
                    test.Name = model.EditingTestName;
                if (String.IsNullOrEmpty(model.EditingTestIcon))
                    test.Icon =
                        "https://img.freepik.com/premium-vector/clipboard-with-checklist-flat-style_183665-74.jpg?w=1060";
                if (!String.IsNullOrEmpty(model.EditingTestIcon))
                    test.Icon = model.EditingTestIcon;
                _dbContext.Tests.Update(test);
                _dbContext.SaveChanges();


                var questions = _dbContext.Questions
                    .Include(q => q.Answers)
                    .Where(q => q.TestId == test.Id)
                    .ToList();
                ProfileTestsModel newModel = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    EditingTestId = test.Id,
                    Questions = questions
                };


                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (newModel.Login == userLogin)
                    {
                        newModel.IsPersonal = true;
                    }
                }

                return View("EditQuestions", newModel);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult AddQuestionView([FromRoute] String id)
    {
        var user = _dbContext.Users.FirstOrDefault(u =>
            u.Id == HttpContext.Session.GetString("authUserId"));
        try
        {
            if (user is not null)
            {
                var test = _dbContext.Tests.Include(t => t.Questions).ThenInclude(q => q.Answers)
                    .FirstOrDefault(t => t.Id == id);
                if (test == null)
                {
                    return NotFound();
                }

                var testQuestions = _dbContext.Questions.Where(q => q.TestId == test.Id).ToList();
                ProfileTestsModel model = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    EditingTestId = test.Id,
                    EditingQuestion = new Question()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = String.Empty,
                        TestId = id,
                        Answers = new List<QuestionAnswer>
                        {
                            new(),
                            new(),
                            new(),
                            new()
                        }
                    },
                };


                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (model.Login == userLogin)
                    {
                        model.IsPersonal = true;
                    }
                }

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    public IActionResult AddQuestion(ProfileTestsModel model)
    {
        var user = _dbContext.Users.FirstOrDefault(u =>
            u.Id == HttpContext.Session.GetString("authUserId"));
        try
        {
            if (user is not null)
            {
                var test = _dbContext.Tests.Include(t => t.Questions).ThenInclude(q => q.Answers)
                    .FirstOrDefault(t => t.Id == model.EditingQuestion.TestId);
                if (test == null)
                {
                    return NotFound();
                }

                var questionId = Guid.NewGuid().ToString();

                List<QuestionAnswer> Answers = new();
                foreach (var answer in model.EditingQuestion.Answers)
                {
                    if (answer.Text != null && answer.Text != String.Empty)
                    {
                        var newAnswer = new QuestionAnswer()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IsRight = answer.IsRight,
                            Question = model.EditingQuestion,
                            QuestionId = questionId,
                            Text = answer.Text
                        };
                        Answers.Add(newAnswer);
                    }
                }

                var question = new Question()
                {
                    Id = questionId,
                    Test = test,
                    Text = model.EditingQuestion.Text,
                    TestId = test.Id,
                    Answers = Answers,
                };

                _dbContext.Questions.Add(question);

                foreach (var answer in Answers)
                {
                    _dbContext.Answers.Add(answer);
                }

                _dbContext.SaveChanges();

                var questions = _dbContext.Questions
                    .Include(q => q.Answers)
                    .Where(q => q.TestId == test.Id)
                    .ToList();
                ProfileTestsModel newModel = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role,
                    EditingTestId = test.Id,
                    Questions = questions
                };


                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (newModel.Login == userLogin)
                    {
                        newModel.IsPersonal = true;
                    }
                }

                return View("EditQuestions", newModel);
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return NotFound();
        }
    }

    public IActionResult DeleteQuestion([FromRoute] String id)
    {
        var userId = HttpContext.Session.GetString("authUserId");
        try
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var questionId = id;
            var question = _dbContext.Questions.FirstOrDefault(q => q.Id == questionId);
            var answers = _dbContext.Answers.Where(a => a.QuestionId == questionId).ToList();
            if (question is not null)
            {
                _dbContext.Questions.Remove(question);
                foreach (var answer in answers)
                {
                    _dbContext.Answers.Remove(answer);
                }

                _dbContext.SaveChanges();
            }

            var test = _dbContext.Tests.Include(t => t.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.Id == question.TestId);
            var questions = _dbContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.TestId == test.Id)
                .ToList();
            ProfileTestsModel newModel = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                EditingTestId = test.Id,
                Questions = questions,
                EditingTestName = test.Name,
                EditingTestDescription = test.Description,
                EditingTestIcon = test.Icon
            };


            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (newModel.Login == userLogin)
                {
                    newModel.IsPersonal = true;
                }
            }

            return View("EditQuestions", newModel);
        }
        catch
        {
            return NotFound();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult CreateTest(MentorTestModel model)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Login == model.Login);
        if (user is not null)
        {
            if (!_dbContext.Tests.Any(t => t.Name == model.TestTitle))
            {
                _dbContext.Tests.Add(new Test()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.TestTitle,
                    Description = model.TestDescription,
                    MentorId = user.Id,
                    Icon = model.TestIcon ??
                           "https://img.freepik.com/premium-vector/clipboard-with-checklist-flat-style_183665-74.jpg?w=1060",
                    Journals = null!,
                    Mentor = user
                });
                _dbContext.SaveChanges();
            }

            ProfileTestsModel newModel = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Tests = _dbContext.Tests.Where(t => t.MentorId == user.Id).AsEnumerable().Select(t =>
                    new MentorTestModel()
                    {
                        TestIcon = t.Icon,
                        TestDescription = t.Description,
                        Id = t.Id,
                        MentorId = t.MentorId,
                        TestTitle = t.Name,
                        Questions = null!
                    }).ToList()
            };

            return View("ProfileTests", newModel);
        }

        return NotFound();
    }

    [HttpPost]
    public RedirectToActionResult AuthUser()
    {
        StringValues loginValues = Request.Form["user-login"];
        if (loginValues.Count == 0)
        {
            // no login
            TempData["results"] = "No login";
            return RedirectToAction("Auth", "Account");
        }

        String login = loginValues[0] ?? "";

        StringValues passwordValues = Request.Form["user-password"];
        if (passwordValues.Count == 0)
        {
            // no password
            TempData["results"] = "No password";
            return RedirectToAction("Auth", "Account");
        }

        String password = passwordValues[0] ?? "";

        User? user = _dbContext.Users.FirstOrDefault(u => u.Login == login);
        if (user is not null)
        {
            if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
            {
                HttpContext.Session.SetString("authUserId", user.Id);
                if (user.Role == "Student")
                {
                    return RedirectToAction("Main", "Home");
                }
                else
                {
                    return RedirectToAction("ProfileTestsEdit", "Account", new { id = user.Login });
                }
            }
        }

        // no user
        TempData["results"] = "Wrong login or password";
        return RedirectToAction("Auth", "Account");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}