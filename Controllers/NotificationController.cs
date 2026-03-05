using EndguinField.Models;
using EndguinField.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EndguinField.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationController(INotificationService notificationService, IUserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = _userService.GetCurrentUserId() ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 20); // Get last 20
            return Ok(notifications);
        }
        
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userService.GetCurrentUserId() ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(0);
            }
            
            var hasUnread = await _notificationService.HasUnreadNotifications(userId);
            // This could be improved to return count, but bool is okay for now
            return Ok(new { hasUnread });
        }
    }
}
