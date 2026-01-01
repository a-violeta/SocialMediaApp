using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    //avand authorize doar un user logat poate face un grup
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        //afisarea tuturor grupurilor

        public IActionResult Index()
        {
            var groups = db.Groups
                           .Include(g => g.Users)
                           .ToList();

            return View(groups);
        }

        //get: creare grup

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //post: creare grup + moderator

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            if (!ModelState.IsValid)
            {
                return View(group);
            }

            // salvam grupul
            db.Groups.Add(group);
            await db.SaveChangesAsync();

            // luam utilizatorul curent
            var userId = _userManager.GetUserId(User);

            // creatorul devine moderator
            var groupUser = new GroupUser
            {
                GroupId = group.Id,
                UserId = userId,
                IsModerator = true
            };

            db.GroupUsers.Add(groupUser);
            await db.SaveChangesAsync();

            // redirect la pagina grupului
            return RedirectToAction("Show", new { id = group.Id });
        }

        //afisare grup
        public IActionResult Show(int id)
        {
            var group = db.Groups
                .Include(g => g.Users)
                    .ThenInclude(gu => gu.User)
                .Include(g => g.Messages)
                    .ThenInclude(m => m.User)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            bool isModerator = db.GroupUsers.Any(gu =>
                gu.GroupId == id &&
                gu.UserId == _userManager.GetUserId(User) &&
                gu.IsModerator);

            ViewBag.IsModerator = isModerator;

            //moderatorul vede cine vrea sa intre in grup
            if (isModerator)
            {
                var joinRequests = db.GroupJoinRequests
                    .Include(r => r.User)
                    .Where(r => r.GroupId == id)
                    .ToList();

                ViewBag.JoinRequests = joinRequests;

            }

            return View(group);
        }

        //editare grup

        // GET: afisare form editare
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var group = await db.Groups
                                .Include(g => g.Users)
                                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // doar moderatorul poate edita
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            return View(group);
        }

        // POST: salvare modificari
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Group group)
        {
            if (!ModelState.IsValid)
                return View(group);

            var existingGroup = await db.Groups
                                        .Include(g => g.Users)
                                        .FirstOrDefaultAsync(g => g.Id == group.Id);

            if (existingGroup == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!existingGroup.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            // actualizare campuri
            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;

            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = group.Id });
        }

        //join, adica face cererea lui de a se alatura grupului

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            // verificam daca e deja membru
            bool alreadyMember = db.GroupUsers
                .Any(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (alreadyMember)
                return BadRequest();

            // verificam daca exista deja cerere
            bool alreadyRequested = db.GroupJoinRequests
                .Any(r => r.GroupId == groupId && r.UserId == userId);

            if (alreadyRequested)
                return RedirectToAction("Show", new { id = groupId });

            var request = new GroupJoinRequest
            {
                GroupId = groupId,
                UserId = userId
            };

            db.GroupJoinRequests.Add(request);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = groupId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Approve(int requestId)
        {
            var request = db.GroupJoinRequests.Find(requestId);
            if (request == null) return NotFound();

            var groupUser = new GroupUser
            {
                GroupId = request.GroupId,
                UserId = request.UserId,
                IsModerator = false
            };

            db.GroupUsers.Add(groupUser);
            db.GroupJoinRequests.Remove(request);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = request.GroupId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(int requestId)
        {
            var request = db.GroupJoinRequests.Find(requestId);
            if (request == null) return NotFound();

            int groupId = request.GroupId;

            db.GroupJoinRequests.Remove(request);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = groupId });
        }

        //parasire grup
        //moderatorul nu poate parasi (discutabil daca modificam sau nu)

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var membership = db.GroupUsers
                .FirstOrDefault(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (membership == null)
                return NotFound();

            if (membership.IsModerator)
                return BadRequest("The moderator can not leave the group.");

            db.GroupUsers.Remove(membership);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int groupId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            // verificam daca userul curent este moderatorul
            bool isModerator = db.GroupUsers.Any(gu =>
                gu.GroupId == groupId &&
                gu.UserId == currentUserId &&
                gu.IsModerator);

            if (!isModerator)
                return Forbid();

            var membership = db.GroupUsers
                .FirstOrDefault(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (membership == null)
                return NotFound();

            if (membership.IsModerator)
                return BadRequest("A moderator can not be removed.");

            db.GroupUsers.Remove(membership);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = groupId });
        }

        //stergerea unui grup

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var group = db.Groups
                .Include(g => g.Users)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // verificam daca utilizatorul este moderator
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            return View(group); // view simplu cu mesaj: "Sigur vrei să ștergi grupul?"
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await db.Groups
                .Include(g => g.Users)
                .Include(g => g.Messages)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // verificam daca utilizatorul este moderator
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            // stergem mesajele
            db.GroupMessages.RemoveRange(group.Messages);

            // stergem relatiile cu utilizatorii
            db.GroupUsers.RemoveRange(group.Users);

            // stergem cererile de join (nu prea mai conteaza daca raman sau nu)
            var joinRequests = db.GroupJoinRequests.Where(r => r.GroupId == id);
            db.GroupJoinRequests.RemoveRange(joinRequests);

            // stergem grupul
            db.Groups.Remove(group);

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        //mesaje in grup

        [HttpGet]
        public async Task<IActionResult> Messages(int groupId)
        {
            var group = await db.Groups
                                .Include(g => g.Users)
                                .Include(g => g.Messages)
                                    .ThenInclude(m => m.User)
                                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            bool isMember = group.Users.Any(u => u.UserId == userId);

            if (!isMember) return Forbid();

            ViewBag.IsModerator = group.Users.Any(u => u.UserId == userId && u.IsModerator);

            return View(group); // va folosi Messages.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(int groupId, string textContent)
        {
            if (string.IsNullOrWhiteSpace(textContent))
            {
                TempData["MessageError"] = "The message can not be empty.";
                return RedirectToAction("Messages", new { groupId });
            }

            var userId = _userManager.GetUserId(User);

            bool isMember = db.GroupUsers.Any(gu =>
                gu.GroupId == groupId && gu.UserId == userId);

            if (!isMember)
                return Forbid();

            var message = new GroupMessage
            {
                GroupId = groupId,
                UserId = userId,
                TextContent = textContent,
                CreatedAt = DateTime.Now
            };

            db.GroupMessages.Add(message);
            await db.SaveChangesAsync();

            return RedirectToAction("Messages", new { groupId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await db.GroupMessages
                .Include(m => m.Group)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            bool isModerator = db.GroupUsers.Any(gu => gu.GroupId == message.GroupId && gu.UserId == userId && gu.IsModerator);

            // doar autorul si moderatorul pot sterge
            if (message.UserId != userId && !isModerator)
                return Forbid();

            db.GroupMessages.Remove(message);
            await db.SaveChangesAsync();

            return RedirectToAction("Messages", new { groupId = message.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(int id, int groupId, string textContent)
        {
            var userId = _userManager.GetUserId(User);

            // verificam daca utilizatorul este membru al grupului
            bool isMember = await db.GroupUsers
                .AnyAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (!isMember)
                return Forbid();

            // validare text
            if (string.IsNullOrWhiteSpace(textContent))
            {
                TempData["MessageError"] = "The message can not be empty.";
                return RedirectToAction("Messages", new { groupId });
            }

            var message = await db.GroupMessages
                .FirstOrDefaultAsync(m => m.Id == id && m.GroupId == groupId);

            if (message == null)
                return NotFound();

            // doar autorul poate edita mesajul
            if (message.UserId != userId)
                return Forbid();

            message.TextContent = textContent;
            await db.SaveChangesAsync();

            return RedirectToAction("Messages", new { groupId });
        }

    }
}
