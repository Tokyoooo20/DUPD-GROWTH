using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class UniversityPresidentController : Controller
{
    [HttpGet("University-President")]
    public IActionResult UniversityPresident()
    {
        return View("~/Views/SuperAdmin/University President.cshtml");
    }
}
