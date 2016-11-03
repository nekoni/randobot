using Microsoft.AspNetCore.Mvc;

namespace RandoBot.Service
{
    /// <summary>
    /// The home controller.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The index endpoint.
        /// </summary>
        /// <returns>A view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// The about endpoint.
        /// </summary>
        /// <returns>A view.</returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// The contact endpoint.
        /// </summary>
        /// <returns>A view.</returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// The error endpoint.
        /// </summary>
        /// <returns>A view.</returns>
        public IActionResult Error()
        {
            return View();
        }
    }
}
