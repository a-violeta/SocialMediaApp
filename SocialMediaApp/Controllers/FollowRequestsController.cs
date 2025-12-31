using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.ViewModels;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    public class FollowRequestsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager )
        {
            db = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ApplicationUser? temp = await _userManager.GetUserAsync(User);
            string id = await _userManager.GetUserIdAsync(temp);
            var requests = await db.Follows
            .Include(f => f.Follower)
            .Where(f =>
                f.FollowedId == id)
            .Select(f => new FollowRequestsViewModel
            {
                FollowerId = f.FollowerId,
                FollowedId = f.FollowedId,
                FollowerFirstName = f.Follower.FirstName,
                FollowerLastName = f.Follower.LastName,
                FollowerPfp = f.Follower.ProfilePicture,
                FollowDate = f.Date,
                Accepted = f.Accepted
            })
            .OrderByDescending(f => f.FollowDate)
            .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Accept(string id)
        {
            ApplicationUser? temp = await _userManager.GetUserAsync(User);
            string followedId = await _userManager.GetUserIdAsync(temp);
            var request = await db.Follows
                .Where(f => f.FollowerId == id && 
                f.FollowedId == followedId)
                .FirstOrDefaultAsync();
            if (request == null)
            {
                return NotFound();
            }
            request.Accepted = true;
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser? temp = await _userManager.GetUserAsync(User);
            string followedId = await _userManager.GetUserIdAsync(temp);
            var request = await db.Follows
                .Where(f => f.FollowerId == id &&
                f.FollowedId == followedId)
                .FirstOrDefaultAsync();
            if (request == null)
            {
                return NotFound();
            }
            db.Follows.Remove(request);
            await db.SaveChangesAsync();
            return Ok();
        }
    }
}
