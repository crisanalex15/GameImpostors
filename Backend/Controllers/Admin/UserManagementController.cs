using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Areas.Identity.Data;
using Backend.Services.Auth;
using Backend.Services.Email;

namespace Backend.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            IAuthService authService,
            IEmailService emailService,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _authService = authService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "User ID is required" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Deleted user {UserId}", id);
                return Json(new { success = true });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLockout([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Unlock user
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Unlocked user {UserId}", request.UserId);
                return Json(new { success = true, action = "unlocked" });
            }
            else
            {
                // Lock user for 24 hours
                user.LockoutEnd = DateTimeOffset.UtcNow.AddHours(24);
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Locked user {UserId} for 24 hours", request.UserId);
                return Json(new { success = true, action = "locked" });
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

            // Generate a temporary password
            var tempPassword = GenerateTempPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Reset password for user {UserId}", request.UserId);
                return Json(new { success = true, tempPassword });
            }

            return Json(new { success = false, message = "Failed to reset password" });
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
                var (succeeded, errors) = await _authService.SendVerificationCodeEmailAsync(user.Email!);
                if (succeeded)
                {
                    _logger.LogInformation("Sent verification email to user {UserId}", request.UserId);
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email for user {UserId}", request.UserId);
                return Json(new { success = false, message = "Failed to send verification email" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] UserActionRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            try
            {
                user.SetPasswordResetVerificationCode();
                await _userManager.UpdateAsync(user);
                await _emailService.SendPasswordResetEmailAsync(user.Email!, user.ResetPasswordCode!);
                _logger.LogInformation("Sent password reset email to user {UserId}", request.UserId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email for user {UserId}", request.UserId);
                return Json(new { success = false, message = "Failed to send password reset email" });
            }
        }

        private string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class UserActionRequest
    {
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }

    public class UserManagementViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
    }
}

