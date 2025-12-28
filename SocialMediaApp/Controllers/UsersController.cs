using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;
using System.IO;

namespace SocialMediaApp.Controllers
{

    public class UsersController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _webHostEnvironment = webHostEnvironment;
        }

        //admin poate vedea toti utilizatorii
        //nu stiu daca ajuta neaparat cu ceva
        
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = db.Users.OrderBy(u => u.UserName);

            ViewBag.UsersList = users;

            return View();
        }

        //cautare user dupa o bucata din nume/prenume sau nume complet, mai multe cuvinte
        //afiseaza orice user (privat/public) momentan

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<ApplicationUser>());
            }

            // spargem textul introdus în cuvinte
            var words = query
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var usersQuery = db.Users.AsQueryable();

            foreach (var word in words)
            {
                string w = word;

                usersQuery = usersQuery.Where(u =>
                    (u.FirstName != null && u.FirstName.Contains(w)) ||
                    (u.LastName != null && u.LastName.Contains(w)));
            }

            var users = usersQuery
                .OrderBy(u => u.FirstName)
                .Take(20)
                .ToList();

            ViewBag.Query = query;
            return View(users);
        }


        //afisare user dupa id
        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser? user = db.Users
                .Include(u => u.Posts)
                .Where(u => u.Id == id)
                .FirstOrDefault();

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                return View(user);
            }
        }



        //editare user dupa id nu e finalizata
        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            string? id = _userManager.GetUserId(User);
            ApplicationUser? user = db.Users.Find(id);
            ViewBag.User = user;
            if (user is null)
            {
                return NotFound();
            }
            var newData = await db.Users
            .Where(u => u.Id == id)
            .Select(u => new EditProfileViewModel
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Description = u.Description,
                ProfileVisibility = u.ProfileVisibility,
                ExistingProfilePicture = u.ProfilePicture
            }).FirstOrDefaultAsync();
            return View(newData);

        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditProfileViewModel newData)
        {
            if (!ModelState.IsValid)
            {
                return View(newData);
            }
            string? id = _userManager.GetUserId(User);
            ApplicationUser? user = db.Users
                .Include(u => u.Posts)
                .Where(u => u.Id == id)
                .FirstOrDefault();

            if (user is null)
            {
                return NotFound();
            }

            else
            {
                if (ModelState.IsValid)
                {
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;

                    string pfpFileName = newData.ExistingProfilePicture;
                    if (newData.ProfilePicture != null)
                    {
                        string pfpSavePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Profiles");

                        pfpFileName = Guid.NewGuid().ToString() + "_" + newData.ProfilePicture.FileName;

                        string pfpFilePath = Path.Combine(pfpSavePath, pfpFileName);

                        using (var fileStream = new FileStream(pfpFilePath, FileMode.Create))
                        {
                            await newData.ProfilePicture.CopyToAsync(fileStream);
                        }
                    }
                    user.ProfilePicture = Path.Combine("images", "Profiles", pfpFileName);
                    user.ProfileVisibility = newData.ProfileVisibility;
                    user.Description =  newData.Description;

                    await db.SaveChangesAsync();

                }
                return RedirectToAction("Show", "Users", new { id });
            }
        }
        /*
        [HttpPost]
        public IActionResult Delete(string id)
        {

            //teoretic mai trebuie sterse si toate mesajele lui din diferite grupuri sau??
            //in plus trebuie sterse si instantele lui de grup user?
            //si toate intrarile din tabelul follows unde apare el?
            var user = db.ApplicationUsers
                            .Include(u => u.Posts)
                            .Include(u => u.Comments)
                            .Include(u => u.Likes)
                            .Where(u => u.Id == id)
                            .First();

            // Delete user comments

            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // Delete user likes
            if (user.Likes.Count > 0)
            {
                foreach (var l in user.Likes)
                {
                    db.Likes.Remove(l);
                }
            }

            // Delete user posts
            if (user.Posts.Count > 0)
            {
                foreach (var post in user.Posts)
                {
                    db.Posts.Remove(post);
                }
            }

            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");

        }
        */

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
