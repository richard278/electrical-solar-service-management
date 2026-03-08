using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers
{
    [Authorize] // cualquiera autenticado
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
