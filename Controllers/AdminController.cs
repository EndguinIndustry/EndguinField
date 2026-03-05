using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace EndguinField.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly ICommentService _commentService;
        private readonly ITagSchemaServices _tagService;

        public AdminController(IUserService userService, IEventService eventService, ICommentService commentService, ITagSchemaServices tagService)
        {
            _userService = userService;
            _eventService = eventService;
            _commentService = commentService;
            _tagService = tagService;
        }

        private async Task<bool> IsAdminAsync()
        {
            var userId = _userService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return false;
            var user = await _userService.GetUserByIdAsync(userId);
            return user != null && user.Role == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Index", "Home");

            var users = await _userService.GetAllUsersAsync();
            var events = await _eventService.GetAllEventsAsync();
            var tags = await _tagService.GetAllAsync();
            
            ViewBag.UserCount = users.Count;
            ViewBag.EventCount = events.Count;
            ViewBag.TagCount = tags.Count;
            
            return View();
        }

        public async Task<IActionResult> Users(string search = "")
        {
            if (!await IsAdminAsync()) return RedirectToAction("Index", "Home");

            var users = await _userService.GetAllUsersAsync();
            
            if (!string.IsNullOrEmpty(search))
            {
               users = users.Where(u => (!string.IsNullOrEmpty(u.Name) && u.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) || 
                                        (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string id, DateTime? banUntil)
        {
            if (!await IsAdminAsync()) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (banUntil.HasValue)
            {
                user.BanUntil = banUntil.Value.AddHours(7);
            }
            else
            {
                user.BanUntil = null;
            }

            await _userService.UpdateUserAsync(id, user);

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> UnbanUser(string id)
        {
             if (!await IsAdminAsync()) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.BanUntil = null;
            await _userService.UpdateUserAsync(id, user);

             return RedirectToAction(nameof(Users));
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
             if (!await IsAdminAsync()) return Unauthorized();
             await _userService.DeleteUserAsync(id);
             return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRole(string id)
        {
             if (!await IsAdminAsync()) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.Role = user.Role == "Admin" ? "User" : "Admin";
            await _userService.UpdateUserAsync(id, user);

             return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Events(string search = "", string status = "")
        {
            if (!await IsAdminAsync()) return RedirectToAction("Index", "Home");

            var events = await _eventService.GetAllEventsAsync();

             if (!string.IsNullOrEmpty(search))
            {
               events = events.Where(e => (!string.IsNullOrEmpty(e.Name) && e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) || 
                                        (!string.IsNullOrEmpty(e.Description) && e.Description.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                events = events.Where(e => e.Status == status).ToList();
            }

            events = events.OrderByDescending(e => e.StartTime).ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;

            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            if (!await IsAdminAsync()) return Unauthorized();
            await _eventService.DeleteEventAsync(id);
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        public async Task<IActionResult> CancelEvent(string id)
        {
            if (!await IsAdminAsync()) return Unauthorized();
            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt != null)
            {
                evt.Status = "Cancelled";
                await _eventService.UpdateEventAsync(id, evt);
            }
            return RedirectToAction(nameof(Events));
        }

        public async Task<IActionResult> Tags()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Index", "Home");
            var tags = await _tagService.GetAllAsync();
            return View(tags); 
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag(string tagName)
        {
            if (!await IsAdminAsync()) return Unauthorized();
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                var existingWrapper = await _tagService.GetAllAsync();
                if (!existingWrapper.Any(t => t.TagName.Equals(tagName.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    var newTag = new Tag { TagName = tagName.Trim() };
                    // TagSchemaServices CreateAsync takes TagModel aka Tag
                    await _tagService.CreateAsync(newTag);
                }
            }
            return RedirectToAction(nameof(Tags));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTag(string id)
        {
            if (!await IsAdminAsync()) return Unauthorized();
            if (!string.IsNullOrEmpty(id))
            {
                await _tagService.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Tags));
        }
    }
}
