using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoInstalacion.AppWeb.Controllers
{
    public class BaseController : Controller
    {
        // Este Index NO debe ser endpoint de MVC
        [NonAction]
        public virtual IActionResult Index() => View();

        protected int UserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        protected string UserNombre =>
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        protected string UserRol =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        protected string UserFoto =>
            User.FindFirstValue("FotoUrl") ?? string.Empty;

    }
}
