using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VprieController : Controller
{
    [HttpGet("VPRIE")]
    public IActionResult VPRIE()
    {
        return View("~/Views/SuperAdmin/VPRIE.cshtml");
    }
}
