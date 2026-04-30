using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class ChancellorController : Controller
{
    [HttpGet("Chancellor")]
    public IActionResult Chancellor()
    {
        return View("~/Views/SuperAdmin/Chancellor.cshtml");
    }
}
