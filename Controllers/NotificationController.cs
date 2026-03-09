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

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 20); 
            return Ok(notifications);
        }
        
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] string id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userService.GetCurrentUserId() ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _notificationService.MarkAllAsReadAsync(userId);
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
            return Ok(new { hasUnread });
        }
    }
}
