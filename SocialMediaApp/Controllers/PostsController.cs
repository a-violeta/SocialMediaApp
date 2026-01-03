using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Controllers
{
    public class PostsController(ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment = environment;

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
                                 .Include(a => a.User) // userul care a scris articolul
                                 .Include(a => a.Images)
                                 .Include(a => a.Videos)
                                 .Include(a => a.WhoLiked)
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
        public async Task<IActionResult> New()
        {
            AddPostViewModel post = new AddPostViewModel();

            return View(post);
        }

        // Se adauga postarea in baza de date
        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> New(AddPostViewModel newPost)
        {
            var user = await _userManager.GetUserAsync(User);

            Post post = new Post
            {
                TextContent = newPost.TextContent,
                Date = DateTime.Now,
                UserId = user.Id
            };

            await db.Posts.AddAsync(post);
            foreach (IFormFile image in newPost.Images)
            {
                string? imageFile = null;
                if (image != null)
                {
                    string savePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Posts");
                    imageFile = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string fullPath = Path.Combine(savePath, imageFile);
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    continue;
                }
                post.Images.Add(new Image { ImageUrl = imageFile });
            }

            foreach (IFormFile video in newPost.Videos)
            {
                string? videoFile = null;
                if (video != null)
                {
                    string savePath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "Posts");
                    videoFile = Guid.NewGuid().ToString() + "_" + video.FileName;
                    string fullPath = Path.Combine(savePath, videoFile);
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await video.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    continue;
                }
                post.Videos.Add(new Video { VideoUrl = videoFile });
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Show", "Posts", new {id = post.Id});
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetMedia(int id)
        {
            var post = db.Posts
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Images = p.Images.Select(i => new
                    {
                        Url = Url.Content($"~/images/Posts/{i.ImageUrl}")
                    }),
                    Videos = p.Videos.Select(v => new
                    {
                        Url = Url.Content($"~/videos/Posts/{v.VideoUrl}")
                    })
                })
                .FirstOrDefault();

            if (post == null)
                return NotFound();

            return Json(post);
        }
    }
}
