using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{

    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
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

        //afisare user dupa id
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ShowAsync(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.Roles = roles;

                ViewBag.UserCurent = await _userManager.GetUserAsync(User);

                return View(user);
            }
        }

        //editare user dupa id nu e finalizata

        /*
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            // doar userul propriu are voie
            if (id != _userManager.GetUserId(User))
            {
                return Forbid(); // sau Unauthorized()
            }

            else
            {
                //user isi poate modifica chiar orice?
                //sa nu strice baza de date
            }

        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser? user = db.Users.Find(id);

            // doar userul propriu are voie
            if (id != _userManager.GetUserId(User))
            {
                return Forbid(); // sau Unauthorized()
            }

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;
                    user.PhoneNumber = newData.PhoneNumber;
                    user.ProfilePicture = newData.ProfilePicture;
                    user.ProfileVisibility = newData.ProfileVisibility;
                    user.Description =  newData.Description;

                    //aici cu rolul lui ce fac? as vrea sa se pastreze ca nu se face singur ce vrea el

                    db.SaveChanges();

                }

                user.AllRoles = GetAllRoles();
                return RedirectToAction("Index");
            }
        }
        */

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
