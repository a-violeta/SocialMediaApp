using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
//using SocialMediaApp.Services;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class CommentsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager//,
        /*IAIContentModerationService moderationService*/) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        //private readonly IAIContentModerationService _moderationService = moderationService;

        //moderation service fusese prima incercare de companion ai

        //new comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(int PostId, string Content)
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["message"] = "The comment cannot be empty.";
                TempData["messageType"] = "danger";
                return RedirectToAction("Show", "Posts", new { id = PostId });
            }

            //var moderationResult = await _moderationService.CheckTextAsync(Content);

            //if (!moderationResult.IsAllowed)
            //{
            //    TempData["message"] = moderationResult.Reason ?? "The comment contains inappropriate content.";
            //    TempData["messageType"] = "danger";

            //    return RedirectToAction("Show", "Posts", new { id = PostId });
            //}

            var userId = _userManager.GetUserId(User);

            var comment = new Comment
            {
                PostId = PostId,
                UserId = userId,
                Content = Content
            };

            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            TempData["message"] = "The comment has been posted.";
            TempData["messageType"] = "success";

            return RedirectToAction("Show", "Posts", new { id = PostId });
        }

        //edit comment
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

        //edit comment
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
                ModelState.Remove("Content");//fara asta nu se suprascria mesajul meu de eroare

                ModelState.AddModelError(
                    "Content",
                    "Comment can not be empty."
                    );

                return View(comment);
            }

            //var moderationResult = await _moderationService.CheckTextAsync(Content);

            //if (!moderationResult.IsAllowed)
            //{
            //    TempData["message"] = "Comment is not approved. Please rethink.";
            //    TempData["messageType"] = "danger";
            //    return View(comment);
            //}

            comment.Content = Content;
            await db.SaveChangesAsync();

            TempData["message"] = "Comment has been edited.";
            TempData["messageType"] = "success";

            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }

        //delete comment
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

            TempData["message"] = "Comment has been deleted.";
            TempData["messageType"] = "success";

            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }
    }
}
