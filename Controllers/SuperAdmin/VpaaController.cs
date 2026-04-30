using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpaaController : Controller
{
    [HttpGet("VPAA")]
    public IActionResult VPAA()
    {
        return View("~/Views/SuperAdmin/VPAA.cshtml");
    }
}
