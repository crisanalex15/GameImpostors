using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Areas.Identity.Data;
using Backend.Services.Auth;
using Backend.Services.Email;

namespace Backend.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuthService authService,
            IEmailService emailService,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var usersWithRoles = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserManagementViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            ViewBag.TotalUsers = users.Count;
            ViewBag.AdminUsers = usersWithRoles.Count(u => u.Roles.Contains("Admin"));
            ViewBag.VerifiedUsers = users.Count(u => u.IsEmailVerified);
            ViewBag.CurrentUser = await _userManager.GetUserAsync(User);

            return View(usersWithRoles);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                _logger.LogInformation("Removed admin role from user {UserId}", request.UserId);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation("Added admin role to user {UserId}", request.UserId);
            }

            return Json(new { success = true, isAdmin = !isAdmin });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser?.Id)
            {
                return Json(new { success = false, message = "Cannot delete your own account" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Deleted user {UserId}", request.UserId);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete user" });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (user.IsEmailVerified)
            {
                return Json(new { success = false, message = "Email already verified" });
            }

            // Manual email verification
            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.MarkEmailAsVerified();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Manually verified email for user {UserId}", request.UserId);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to verify email" });
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationEmail([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (user.IsEmailVerified)
            {
                return Json(new { success = false, message = "Email already verified" });
            }

            try
            {
                var (succeeded, errors) = await _authService.SendVerificationCodeEmailAsync(user.Email);
                if (succeeded)
                {
                    _logger.LogInformation("Sent verification email to user {UserId}", request.UserId);
                    return Json(new { success = true, message = "Verification email sent successfully" });
                }
                else
                {
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to user {UserId}", request.UserId);
                return Json(new { success = false, message = "Failed to send verification email" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            try
            {
                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Send password reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, token);

                _logger.LogInformation("Sent password reset email to user {UserId}", request.UserId);
                return Json(new { success = true, message = "Password reset email sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to user {UserId}", request.UserId);
                return Json(new { success = false, message = "Failed to send password reset email" });
            }
        }
    }

    public class UserManagementViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserActionRequest
    {
        public string UserId { get; set; }
    }
}
