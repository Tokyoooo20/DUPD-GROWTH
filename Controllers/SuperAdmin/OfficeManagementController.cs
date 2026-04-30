using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class OfficeManagementController : Controller
{
    [HttpGet("OfficeManagement")]
    public IActionResult OfficeManagement()
    {
        return View("~/Views/SuperAdmin/OfficeM.cshtml");
    }
}
