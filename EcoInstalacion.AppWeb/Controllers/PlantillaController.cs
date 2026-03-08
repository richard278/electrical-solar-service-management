using Microsoft.AspNetCore.Mvc;

namespace EcoInstalacion.AppWeb.Controllers
{
    public class PlantillaController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult EnviarClave(string correo, string clave)
        {
            ViewData["Correo"] = correo;
            ViewData["Clave"] = clave;
            ViewData["Url"] = $"{this.Request.Scheme}://{this.Request.Host}";
            return View();
        }

        public IActionResult RestablecerClave(string clave)
        {
            ViewData["Clave"] = clave;
            return View();
        }
    }
}
