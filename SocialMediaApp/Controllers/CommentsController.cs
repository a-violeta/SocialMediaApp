/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    public class CommentsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;
        public IActionResult Index()
        {
            return View();
        }
    }
}

*/
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class CommentsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;


        // POST: Add new comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(int PostId, string Content)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["CommentError"] = "The comment cannot be empty.";
                return RedirectToAction("Show", "Posts", new { id = PostId });
            }

            var comment = new Comment
            {
                PostId = PostId,
                UserId = userId,
                Content = Content
            };

            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = PostId });
        }

        // get, edit comment
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await db.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (comment.UserId == null || comment.UserId != currentUserId)
                return Forbid();

            return View(comment);
        }

        // post, edit comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Content)
        {
            var comment = await db.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (comment.UserId == null || comment.UserId != currentUserId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["CommentError"] = "The comment cannot be empty.";
                return RedirectToAction("Show", "Posts", new { id = comment.PostId });
            }

            comment.Content = Content;
            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }

        // post, delete comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await db.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            if (!(isAdmin || (comment.UserId != null && comment.UserId == currentUserId)))
                return Forbid();

            db.Comments.Remove(comment);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }
    }
}
