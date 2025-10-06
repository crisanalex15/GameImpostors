using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.API
{
    [Authorize]
    public class SwaggerController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}

