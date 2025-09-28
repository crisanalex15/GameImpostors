using System.Diagnostics;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Backend.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult GetToApi()
        {
            return Redirect("~/swagger");
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
