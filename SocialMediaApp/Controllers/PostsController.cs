using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        /*
        public PostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        */

        //am lasat un view gol posts/index, dar nu sunt sigura ca e folositor
        /*
        public IActionResult Index()
        {
            return View();
        }
        */

        // Se afiseaza o postare in functie de id
        // impreuna cu user ul
        // In plus sunt preluate si toate comentariile asociate
        // doar ca afisarea continutului nu merge inca
        
        // [HttpGet]

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Show(int id)
        {
            Post? post = await db.Posts
                                 .Include(a => a.Comments)
                                 .Include(a => a.User) // userul care a scris articolul
                                 .Include(a => a.Comments)
                                    .ThenInclude(c => c.User) // userii care au scris comentariile
                                 .Where(a => a.Id == id)
                                 .FirstOrDefaultAsync();

            if (post is null)
            {
                return NotFound();
            }

            //SetAccessRights();
            //metoda asta e din lab 10 dar n am adaugat o inca

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(post);
        }

        // Formularul in care se vor completa datele unei postari
        // momentan nu suporta partea video si imagine
        
        // [HttpGet] se executa implicit

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        // Se adauga postarea in baza de date
        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Post post)
        {
            post.Date = DateTime.Now;

            //Id-ul utilizatorului care posteaza
            post.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = post.Id });
            }

            else
            {
                return View(post);
            }
        }
    }
}
