using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    // Acest controller NU are [Authorize] - va fi blocat de politica globală
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        // Această acțiune va fi blocată de politica globală de autorizare
        public IActionResult PublicPage()
        {
            return Json(new
            {
                message = "Această pagină ar trebui să fie blocată!",
                timestamp = DateTime.Now,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }

        // Această acțiune permite accesul anonim explicit
        [AllowAnonymous]
        public IActionResult AnonymousPage()
        {
            return Json(new
            {
                message = "Această pagină permite accesul anonim",
                timestamp = DateTime.Now,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
    }
}
