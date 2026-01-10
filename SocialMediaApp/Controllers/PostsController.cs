using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
//using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Controllers
{
    public class PostsController(ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment//,
        /*AiModerationService moderationService*/) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment = environment;
        //private readonly AiModerationService _moderationService = moderationService;

        //fusese prima varianta de companion ai

        //afisare postare
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Show(int id)
        {
            Post? post = await db.Posts
                                 .Include(a => a.User) // userul care a scris articolul
                                 .Include(a => a.Images)
                                 .Include(a => a.Videos)
                                 .Include(a => a.WhoLiked)
                                 .Include(a => a.Comments.OrderByDescending(c => c.CreatedAt))
                                    .ThenInclude(c => c.User) // userii care au scris comentariile
                                 .Where(a => a.Id == id)
                                 .FirstOrDefaultAsync();
            if (post is null)
            {
                return NotFound();
            }
            var currentUserId = _userManager.GetUserId(User);
            if (post.User.ProfileVisibility == "private")
            {
                bool isMe = post.UserId == _userManager.GetUserId(User);
                var connectedUser = await _userManager.GetUserAsync(User);
                bool isFollowingConfirmed;
                var follow = await db.Follows
                    .Where(f => f.FollowerId == currentUserId && f.FollowedId == post.UserId)
                    .FirstOrDefaultAsync();

                bool isFollowing = follow != null;
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

                if (!amIAdmin && !isFollowingConfirmed && !isMe)
                {
                    return RedirectToAction("Show", "Users", new { id = post.UserId });
                }
            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            ViewBag.HasLiked = post.WhoLiked.Any(l => l.UserId == currentUserId);

            return View(post);
        }

        //postare noua
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> New()
        {
            AddPostViewModel post = new AddPostViewModel();

            return View(post);
        }

        //postare noua        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> New(AddPostViewModel newPost)
        {
            if (!ModelState.IsValid)
            {
                return View(newPost);
            }

            //bool isAllowed = await _moderationService.IsPostAllowedAsync(newPost.TextContent);
            //if (!isAllowed)
            //{
            //    ModelState.AddModelError("TextContent", "Please review before posting.");
            //    return View(newPost);
            //}

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

            //tempdata[message]=...
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

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await db.Posts
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (post.UserId != currentUserId)
                return Forbid();

            var model = new AddPostViewModel
            {
                TextContent = post.TextContent,
                //ne trebuie media curent ca sa stim ce editam
                ExistingImageIds = post.Images.Select(i => i.Id).ToList(),
                ExistingVideoIds = post.Videos.Select(v => v.Id).ToList()
            };

            ViewBag.PostId = post.Id;
            //ca sa avem imaginile/video reale in view
            ViewBag.Post = post;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["message"] = "The post could not be saved.";
                TempData["messageType"] = "danger";
                ViewBag.PostId = id;
                return View(model);
            }

            var post = await db.Posts
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (post.UserId != currentUserId)
                return Forbid();

            //var moderationResult = await _moderationService.CheckTextAsync(model.TextContent);

            //if (!moderationResult.IsAllowed)
            //{
            //    ModelState.AddModelError(
            //        "TextContent",
            //        moderationResult.Reason ?? "The post contains inappropriate content."
            //    );

            //    ViewBag.PostId = id;
            //    ViewBag.Post = post; // ca sa reafisam imaginile, video urile
            //    return View(model);
            //}

            // edit text
            post.TextContent = model.TextContent;
            post.Date = DateTime.Now;

            // stergere imagini selectate
            if (model.RemoveImageIds != null)
            {
                foreach (var imgId in model.RemoveImageIds)
                {
                    var img = post.Images.FirstOrDefault(i => i.Id == imgId);
                    if (img != null)
                    {
                        // stergere fizica din wwwroot
                        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Posts", img.ImageUrl);
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);

                        post.Images.Remove(img);
                        db.Images.Remove(img);
                    }
                }
            }

            // stergere videoclipuri selectate
            if (model.RemoveVideoIds != null)
            {
                foreach (var vidId in model.RemoveVideoIds)
                {
                    var vid = post.Videos.FirstOrDefault(v => v.Id == vidId);
                    if (vid != null)
                    {
                        // stergere fizica din wwwroot
                        string path = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "Posts", vid.VideoUrl);
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);

                        post.Videos.Remove(vid);
                        db.Videos.Remove(vid);
                    }
                }
            }


            // adaugare imagini noi (optional)
            foreach (var image in model.Images ?? Enumerable.Empty<IFormFile>())
            {
                string fileName = Guid.NewGuid() + "_" + image.FileName;
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Posts", fileName);

                using var fs = new FileStream(path, FileMode.Create);
                await image.CopyToAsync(fs);

                post.Images.Add(new Image { ImageUrl = fileName });
            }

            // adaugare videoclipuri noi (optional)
            foreach (var video in model.Videos ?? Enumerable.Empty<IFormFile>())
            {
                string fileName = Guid.NewGuid() + "_" + video.FileName;
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "Posts", fileName);

                using var fs = new FileStream(path, FileMode.Create);
                await video.CopyToAsync(fs);

                post.Videos.Add(new Video { VideoUrl = fileName });
            }

            await db.SaveChangesAsync();

            TempData["message"] = "The post has been successufully edited.";
            TempData["messageType"] = "success";

            return RedirectToAction("Show", new { id = post.Id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // gaseste postarea cu toate fisierele
            var post = await db.Posts
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            // verifica daca e proprietarul sau admin
            bool isAdmin = User.IsInRole("Admin");

            if (post.UserId != currentUserId && !isAdmin)
                return Forbid();

            // sterge fisierele fizice
            foreach (var img in post.Images)
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Posts", img.ImageUrl);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            foreach (var vid in post.Videos)
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "Posts", vid.VideoUrl);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            // sterge like urile
            var likes = await db.Likes.Where(l => l.PostId == post.Id).ToListAsync();
            db.Likes.RemoveRange(likes);

            // sterge comentariile
            var comments = await db.Comments.Where(c => c.PostId == post.Id).ToListAsync();
            db.Comments.RemoveRange(comments);

            // sterge postarea din db
            db.Posts.Remove(post);
            await db.SaveChangesAsync();

            TempData["message"] = "The post has been successfully removed.";
            TempData["messageType"] = "success";

            return RedirectToAction("Show", "Users", new { id = post.UserId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = _userManager.GetUserId(User);
            var post = await db.Posts
                .Include(p => p.WhoLiked)
                .FirstOrDefaultAsync(p => p.Id == postId);
            bool liked = false;

            if (post == null)
                return NotFound();

            // verifica daca utilizatorul a dat deja like
            var existingLike = post.WhoLiked.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                // unlike
                db.Remove(existingLike);
            }
            else
            {
                // like
                var like = new Likes
                {
                    PostId = postId,
                    UserId = userId
                };
                db.Add(like);
                liked = true;
            }

            await db.SaveChangesAsync();

            // returneaza JSON cu nr actualizat de like uri
            var likesCount = post.WhoLiked.Count;
            return Json(new { likesCount, liked });
        }

    }
}
