// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SocialMediaApp.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext db;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            db = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var dbUser = await _userManager.Users
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
                            .FirstAsync(u => u.Id == user.Id);

            // anonimizam comentariile

            foreach (var comment in dbUser.Comments)
            {
                comment.UserId = null;
            }

            // stergem like-urile

            foreach (var l in dbUser.Likes)
            {
                db.Likes.Remove(l);
            }

            // stergem postarile

            foreach (var post in dbUser.Posts)
            {
                foreach (var comment in post.Comments)
                {
                    db.Comments.Remove(comment);
                }
                foreach (var like in post.WhoLiked)
                {
                    db.Likes.Remove(like);
                }
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
            foreach (var following in dbUser.Follows)
            {
                db.Follows.Remove(following);
            }

            foreach (var follow in dbUser.Followers)
            {
                db.Follows.Remove(follow);
            }

            // stergem join request-urile, calitatea de membru in grupuri
            // si anonimizam mesajele

            foreach (var joinRequest in dbUser.JoinRequests)
            {
                db.GroupJoinRequests.Remove(joinRequest);
            }

            foreach (var group in dbUser.Groups)
            {
                if (group.IsModerator)
                {
                    var userGroup = group.Group;
                    if (userGroup.Users.Count() > 1)
                    {
                        var oldestUser = userGroup.Users.Where(u => u.UserId != dbUser.Id).OrderBy(u => u.JoinDate).First();
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

            foreach (var message in dbUser.Messages)
            {
                message.UserId = null;
            }


            await db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var ProfilePicturePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Profiles", user.ProfilePicture);
                if (System.IO.File.Exists(ProfilePicturePath))
                {
                    System.IO.File.Delete(ProfilePicturePath);
                }
            }
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}
