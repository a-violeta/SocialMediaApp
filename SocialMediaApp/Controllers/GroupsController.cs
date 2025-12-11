using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;

namespace SocialMediaApp.Controllers
{
    public class GroupsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;
        public IActionResult Index()
        {
            return View();
        }
    }
}
