using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

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

        //cautare user dupa o bucata din nume/prenume sau nume complet, mai multe cuvinte
        //afiseaza orice user (privat/public)

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
            ApplicationUser? user = await db.Users
                .Include(u => u.Posts.OrderByDescending(p => p.Date))
                    .ThenInclude(p => p.Comments)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.WhoLiked)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Videos)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Images)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
            if (user is null)
            {
                return NotFound();
            }
            bool isMe = user.Id == _userManager.GetUserId(User);
            var connectedUser = await _userManager.GetUserAsync(User);

            var currentUserId = _userManager.GetUserId(User);

            var follow = await db.Follows
                .Where(f => f.FollowerId == currentUserId && f.FollowedId == user.Id)
                .FirstOrDefaultAsync();

            bool isFollowing = follow != null;

            bool isFollowingConfirmed;


            int followerCount = db.Follows.Where(f =>
            f.FollowedId == user.Id && 
            f.Accepted == true).Count();

            int followingCount = db.Follows.Where(f =>
            f.FollowerId == user.Id &&
            f.Accepted == true).Count();

            if (follow is null)
            {
                isFollowingConfirmed = false;
            }

            else
            {
                isFollowingConfirmed = isFollowing && follow.Accepted;
            }

            bool amIAdmin;
            if (connectedUser is null)
            {
                amIAdmin = false;
            }
            else
            {
                amIAdmin = await _userManager.IsInRoleAsync(connectedUser, "Admin");
            }
            ViewBag.IsMe = isMe;
            ViewBag.AmIAdmin = amIAdmin;
            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsFollowingConfirmed = isFollowingConfirmed;
            ViewBag.FollowerCount = followerCount;
            ViewBag.FollowingCount = followingCount;
            return View(user);

        }

        // editare user
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
                return BadRequest();
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
                    var oldImagePath = user.ProfilePicture;
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

                        // stergem poza de profil veche
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Profiles", oldImagePath);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }
                    }
                    user.ProfilePicture = pfpFileName;

                    user.ProfileVisibility = newData.ProfileVisibility;
                    user.Description =  newData.Description;

                    await db.SaveChangesAsync();

                }
                return RedirectToAction("Show", "Users", new { id });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        { 
            var user = await db.ApplicationUsers
                            .Include(u => u.Posts)
                                .ThenInclude(p => p.Comments)
                            .Include(u => u.Posts)
                                .ThenInclude(p => p.WhoLiked)
                            .Include(u => u.Posts)
                                .ThenInclude(p => p.Images)
                            .Include(u => u.Posts)
                                .ThenInclude(p => p.Videos)
                            .Include(u => u.Comments)
                            .Include(u => u.Likes)
                            .Include(u => u.Followers)
                            .Include(u => u.Follows)
                            .Include(u => u.JoinRequests)
                            .Include(u => u.Groups)
                                .ThenInclude(gu => gu.Group)
                                    .ThenInclude(g => g.Users)
                            .Include(u => u.Messages)
                            .Where(u => u.Id == id)
                            .FirstOrDefaultAsync();

            if (user is null)
            {
                return NotFound();
            }

            // anonimizam comentariile

            foreach (var comment in user.Comments)
            {
                comment.UserId = null;
            }

            // stergem like-urile

            foreach (var l in user.Likes)
            {
                db.Likes.Remove(l);
            }

            // stergem postarile

            foreach (var post in user.Posts)
            {
                // stergem comentariile postarii
                foreach (var comment in post.Comments)
                {
                    db.Comments.Remove(comment);
                }

                // stergem like-urile postarii
                foreach (var like in post.WhoLiked)
                {
                    db.Likes.Remove(like);
                }

                // stergem imaginile postarii
                foreach (var image in post.Images)
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Posts", image.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    db.Images.Remove(image);
                }
                foreach (var video in post.Videos)
                {
                    string videoPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "Posts", video.VideoUrl);
                    if (System.IO.File.Exists(videoPath))
                    {
                        System.IO.File.Delete(videoPath);
                    }
                    db.Videos.Remove(video);
                }
                db.Posts.Remove(post);
            }

            // stergem follow-urile
            foreach (var following in user.Follows)
            {
                db.Follows.Remove(following);
            }

            foreach (var follow in user.Followers)
            {
                db.Follows.Remove(follow);
            }

            // stergem join request-urile, calitatea de membru in grupuri
            // si anonimizam mesajele

            foreach (var joinRequest in user.JoinRequests)
            {
                db.GroupJoinRequests.Remove(joinRequest);
            }

            // daca un user este moderator in vreun grup cand il stergem,
            // punem al doilea cel mai vechi user ca moderator.
            // daca nu mai e niciun alt user, stergem grupul.
            foreach (var group in user.Groups)
            {
                if (group.IsModerator)
                {
                    var userGroup = group.Group;
                    if (userGroup.Users.Count() > 1)
                    {
                        var oldestUser = userGroup.Users.Where(u => u.UserId != user.Id).OrderBy(u => u.JoinDate).First();
                        oldestUser.IsModerator = true;
                        db.GroupUsers.Remove(group);
                    }
                    else
                    {
                        db.GroupUsers.Remove(group);
                        db.Groups.Remove(userGroup);
                    }
                }
            }

            // mesajul va avea doar user null, nu va fi sters
            foreach (var message in user.Messages)
            {
                message.UserId = null;
            }

            // stergem poza de profil 

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var ProfilePicturePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Profiles", user.ProfilePicture);
                if (System.IO.File.Exists(ProfilePicturePath))
                {
                    System.IO.File.Delete(ProfilePicturePath);
                }
            }

            db.ApplicationUsers.Remove(user);

            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Follow(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == id)
                return BadRequest();

            var existing = await db.Follows
                .FirstOrDefaultAsync(f =>
                    f.FollowerId == currentUserId &&
                    f.FollowedId == id);

            if (existing == null)
            {
                db.Follows.Add(new Follows
                {
                    FollowerId = currentUserId,
                    FollowedId = id,
                    Accepted = false,
                    Date = DateTime.Now
                });
            }
            else
            {
                db.Follows.Remove(existing);
            }

            await db.SaveChangesAsync();

            var isFollowing = existing == null;

            return Json(new { isFollowing });
        }

        [Authorize]
        public async Task<IActionResult> Feed()
        {
            var currentUserId = _userManager.GetUserId(User);

            //id urile utilizatorilor urmariti
            var followingIds = await db.Follows
                .Where(f => f.FollowerId == currentUserId && f.Accepted)
                .Select(f => f.FollowedId)
                .ToListAsync();

            //ia postarile lor, incluzand tot necesar
            var posts = await db.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .Include(p => p.WhoLiked)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return View(posts); // Views/Users/Feed.cshtml
        }

    }
}
