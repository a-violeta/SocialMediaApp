using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //daca user ul e logat => feed
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                //potential mesaj de welcome back cu un tempdata?
                return RedirectToAction("Feed", "Users");
            }

            //altfel => landing page, home index (actionname pt redenumire?)
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
