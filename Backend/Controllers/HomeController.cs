using System.Diagnostics;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Backend.Models.Questions;
namespace Backend.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult GetToApi()
        {
            return Redirect("~/swagger");
        }

        public IActionResult Questions()
        {
            var QList = _context.Questions.ToList();
            var WList = _context.WordHiddens.ToList();

            ViewBag.QList = QList;
            ViewBag.WList = WList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(string questionText, string fakeQuestionText, string questionCategory, string difficulty, bool isActive)
        {
            try
            {
                var question = new Question
                {
                    QuestionText = questionText,
                    FakeQuestionText = fakeQuestionText,
                    Category = string.IsNullOrEmpty(questionCategory) ? "General" : questionCategory,
                    Difficulty = int.Parse(difficulty),
                    IsActive = isActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Întrebarea a fost adăugată cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Guid id, string questionText, string fakeQuestionText, string editQuestionCategory, string difficulty, bool isActive)
        {
            try
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    return Json(new { success = false, message = "Întrebarea nu a fost găsită!" });
                }

                question.QuestionText = questionText;
                question.FakeQuestionText = fakeQuestionText;
                question.Category = string.IsNullOrEmpty(editQuestionCategory) ? "General" : editQuestionCategory;
                question.Difficulty = int.Parse(difficulty);
                question.IsActive = isActive;
                question.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Întrebarea a fost actualizată cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            try
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    return Json(new { success = false, message = "Întrebarea nu a fost găsită!" });
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Întrebarea a fost ștearsă cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWord(string word, string wordCategory, string wordDifficulty, bool wordIsActive)
        {
            try
            {
                var wordHidden = new WordHidden
                {
                    Word = word,
                    Category = wordCategory,
                    Difficulty = int.Parse(wordDifficulty),
                    IsActive = wordIsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WordHiddens.Add(wordHidden);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cuvântul a fost adăugat cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWord(Guid id, string word, string wordCategory, string wordDifficulty, bool wordIsActive)
        {
            try
            {
                var wordHidden = await _context.WordHiddens.FindAsync(id);
                if (wordHidden == null)
                {
                    return Json(new { success = false, message = "Cuvântul nu a fost găsit!" });
                }

                wordHidden.Word = word;
                wordHidden.Category = wordCategory;
                wordHidden.Difficulty = int.Parse(wordDifficulty);
                wordHidden.IsActive = wordIsActive;
                wordHidden.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cuvântul a fost actualizat cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWord(Guid id)
        {
            try
            {
                var wordHidden = await _context.WordHiddens.FindAsync(id);
                if (wordHidden == null)
                {
                    return Json(new { success = false, message = "Cuvântul nu a fost găsit!" });
                }

                _context.WordHiddens.Remove(wordHidden);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cuvântul a fost șters cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Eroare: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Index()
        {
            // Obține statistici utilizatori
            var users = _userManager.Users.ToList();
            var totalUsers = users.Count;
            var verifiedUsers = users.Count(u => u.IsEmailVerified);
            var adminUsers = users.Where(u => _userManager.IsInRoleAsync(u, "Admin").Result).Count();
            var recentUsers = users.Count(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30));

            // Statistici pentru utilizatorul curent
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.VerifiedUsers = verifiedUsers;
            ViewBag.AdminUsers = adminUsers;
            ViewBag.RecentUsers = recentUsers;
            ViewBag.CurrentUser = currentUser;
            ViewBag.UserRoles = userRoles;
            ViewBag.UserID = currentUser?.Id;

            var QList = _context.Questions.ToList();
            var WList = _context.WordHiddens.ToList();

            ViewBag.QList = QList;
            ViewBag.WList = WList;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
