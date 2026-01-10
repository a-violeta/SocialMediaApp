using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Responses;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Text.RegularExpressions;//asta contine clasa Group, distictia: SocialMediaApp.Models.Group

namespace SocialMediaApp.Controllers
{
    [Authorize]
    //doar un user logat poate face actiuni pe un grup
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

        //creare grup
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //creare grup + moderator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SocialMediaApp.Models.Group group)
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

            TempData["message"] = "The group has been created.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = group.Id });
        }

        //afisare grup
        public IActionResult Show(int id)
        {
            var group = db.Groups
                .Include(g => g.Users)
                    .ThenInclude(gu => gu.User)
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
            ViewBag.isAdmin = User.IsInRole("Admin");

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
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var group = await db.Groups
                                .Include(g => g.Users)
                                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            //doar moderatorul poate edita
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            return View(group);
        }

        //editare grup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SocialMediaApp.Models.Group group)
        {
            if (!ModelState.IsValid)
                return View(group);

            var existingGroup = await db.Groups
                                        .Include(g => g.Users)
                                        .FirstOrDefaultAsync(g => g.Id == group.Id);

            if (existingGroup == null)
                return NotFound();//grup gol nu are sens?

            var userId = _userManager.GetUserId(User);
            if (!existingGroup.Users.Any(gu => gu.UserId == userId && gu.IsModerator))
                return Forbid();

            // actualizare campuri
            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;

            await db.SaveChangesAsync();
            TempData["message"] = "The group has been edited.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = group.Id });
        }

        //join, cererea de a se alatura grupului
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int groupId)
        {
            var userId = _userManager.GetUserId(User);

            // verificam daca e deja membru
            bool alreadyMember = db.GroupUsers
                .Any(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (alreadyMember)
                return BadRequest();

            // verificam daca exista deja cerere
            bool alreadyRequested = db.GroupJoinRequests
                .Any(r => r.GroupId == groupId && r.UserId == userId);

            if (alreadyRequested)
            {
                TempData["message"] = "Your request is pending.";
                TempData["messageType"] = "success";
                return RedirectToAction("Show", new { id = groupId });
            }

            var request = new GroupJoinRequest
            {
                GroupId = groupId,
                UserId = userId
            };

            db.GroupJoinRequests.Add(request);
            await db.SaveChangesAsync();
            TempData["message"] = "Your request to join has been sent.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int requestId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var request = db.GroupJoinRequests.Find(requestId);

            if (request == null) return NotFound();

            bool isModerator = db.GroupUsers.Any(gu =>
            gu.GroupId == request.GroupId &&
            gu.UserId == currentUserId &&
            gu.IsModerator);

            if (!isModerator)
                return Forbid();//doar moderatorul accepta sau nu persoane in grup

            var groupUser = new GroupUser
            {
                GroupId = request.GroupId,
                UserId = request.UserId,
                JoinDate = DateTime.Now,
                IsModerator = false
            };

            db.GroupUsers.Add(groupUser);
            db.GroupJoinRequests.Remove(request);
            await db.SaveChangesAsync();
            TempData["message"] = "A user has been accepted into the group.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = request.GroupId });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int requestId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var request = db.GroupJoinRequests.Find(requestId);
            if (request == null) return NotFound();
            
            int groupId = request.GroupId;

            var isModerator = db.GroupUsers.Any(gu =>
            gu.GroupId == request.GroupId &&
            gu.UserId == currentUserId &&
            gu.IsModerator);

            if (!isModerator)
                return Forbid();//doar moderatorul accepta sau nu persoane in grup

            db.GroupJoinRequests.Remove(request);
            await db.SaveChangesAsync();
            TempData["message"] = "A user has been denied joining the group.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = groupId });
        }

        //parasire grup
        //cand moderatorul paraseste se alege altul daca exista, altfel se sterge grupul

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int groupId)
        {
            var userId = _userManager.GetUserId(User);

            var membership = await db.GroupUsers
                .Include(gu => gu.Group)
                    .ThenInclude(g => g.Users)
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (membership == null)
                return NotFound();

            var group = membership.Group;//group nu e null

            if (membership.IsModerator)
            {
                //ceilalti membri ai grupului
                var otherMembers = group.Users
                    .Where(u => u.UserId != userId)
                    .OrderBy(u => u.JoinDate)
                    .ToList();

                if (otherMembers.Any())
                {
                    //cel mai vechi devine moderator
                    otherMembers.First().IsModerator = true;
                    TempData["message"] = "You have left your group as moderator.";
                    TempData["messageType"] = "success";
                    db.GroupUsers.Remove(membership);
                }
                else
                {
                    //nu mai e nimeni -> stergem grupul
                    TempData["message"] = "The group has been deleted.";
                    TempData["messageType"] = "success";
                    db.GroupUsers.Remove(membership);
                    db.Groups.Remove(group);
                }
            }
            else
            {
                //userul normal se sterge normal
                TempData["message"] = "You have left the group.";
                TempData["messageType"] = "success";
                db.GroupUsers.Remove(membership);
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int groupId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            //verificam daca userul curent este moderatorul
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
                return BadRequest();

            TempData["message"] = "A user has been removed.";
            TempData["messageType"] = "success";
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
            bool isAdmin = User.IsInRole("Admin");

            //verificam daca utilizatorul este moderator sau admin
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator) && !isAdmin)
                return Forbid();

            return View(group);
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
            bool isAdmin = User.IsInRole("Admin");

            //verificam daca utilizatorul este moderator
            if (!group.Users.Any(gu => gu.UserId == userId && gu.IsModerator) && !isAdmin)
                return Forbid();

            //stergem mesajele
            db.GroupMessages.RemoveRange(group.Messages);

            //stergem relatiile cu utilizatorii
            db.GroupUsers.RemoveRange(group.Users);

            //stergem cererile de join (nu prea mai conteaza daca raman sau nu)
            var joinRequests = db.GroupJoinRequests.Where(r => r.GroupId == id);
            db.GroupJoinRequests.RemoveRange(joinRequests);

            //stergem grupul
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            TempData["message"] = "The group has been deleted.";
            TempData["messageType"] = "success";
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

            //admin va putea vedea mesajele fara sa fie membru
            if (!isMember && !User.IsInRole("Admin")) return Forbid();

            ViewBag.IsModerator = group.Users.Any(u => u.UserId == userId && u.IsModerator);
            ViewBag.isAdmin = User.IsInRole("Admin");

            return View(group); //va folosi Messages.cshtml din Groups
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(int groupId, string textContent)
        {
            if (string.IsNullOrWhiteSpace(textContent))
            {
                TempData["message"] = "The message can not be empty.";
                TempData["messageType"] = "danger";
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
            TempData["message"] = "Message has been sent.";
            TempData["messageType"] = "success";
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
            bool isAdmin = User.IsInRole("Admin");

            //doar autorul, moderatorul si adminii pot sterge
            if (message.UserId != userId && !isModerator && !isAdmin)
                return Forbid();

            db.GroupMessages.Remove(message);
            await db.SaveChangesAsync();
            TempData["message"] = "The message has been deleted.";
            TempData["messageType"] = "success";
            return RedirectToAction("Messages", new { groupId = message.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage(int id, int groupId, string textContent)
        {
            var userId = _userManager.GetUserId(User);

            //verificam daca utilizatorul este membru al grupului
            bool isMember = await db.GroupUsers
                .AnyAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (!isMember)
                return Forbid();

            //validare text
            if (string.IsNullOrWhiteSpace(textContent))
            {
                TempData["message"] = "The message can not be empty.";
                TempData["messageType"] = "danger";
                return RedirectToAction("Messages", new { groupId });
            }

            var message = await db.GroupMessages
                .FirstOrDefaultAsync(m => m.Id == id && m.GroupId == groupId);

            if (message == null)
                return NotFound();

            //doar autorul poate edita mesajul
            if (message.UserId != userId)
                return Forbid();

            message.TextContent = textContent;
            await db.SaveChangesAsync();
            TempData["message"] = "The message has been edited.";
            TempData["messageType"] = "success";
            return RedirectToAction("Messages", new { groupId });
        }

        [HttpGet]
        public IActionResult Search()
        {
            //afiseaza pagina de search goala
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string query, bool searchBy)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<SocialMediaApp.Models.Group>()); //sau un ViewModel

            IQueryable<SocialMediaApp.Models.Group> groups = db.Groups
                .Include(g => g.Users);

            if (searchBy) //true = search by Name
                groups = groups.Where(g => g.Name.Contains(query));
            else //search by Id
            {
                if (int.TryParse(query, out int id))
                    groups = groups.Where(g => g.Id == id);
                else
                    groups = groups.Where(g => false); //id invalid
            }

            var results = await groups.ToListAsync();
            return View("SearchResults", results); //trimitem doar lista de grupuri
        }

    }
}
