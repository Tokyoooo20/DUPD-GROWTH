using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpfaController : Controller
{
    [HttpGet("VPFA")]
    public IActionResult VPFA()
    {
        return View("~/Views/SuperAdmin/VPFA.cshtml");
    }
}
