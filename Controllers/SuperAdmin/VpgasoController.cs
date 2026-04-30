using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpgasoController : Controller
{
    [HttpGet("VPGASO")]
    public IActionResult VPGASO()
    {
        return View("~/Views/SuperAdmin/VPGASO.cshtml");
    }
}
