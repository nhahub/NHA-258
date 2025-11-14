using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTransportation.BLL.DTOs.Notification;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;

namespace SmartTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetAllNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetAllNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponseDto>> GetNotificationById(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                    return NotFound(new { message = $"Notification with ID {id} was not found." });

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notifications by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotificationsByUserId(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notifications by user ID
        /// </summary>
        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetUnreadNotificationsByUserId(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving unread notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<NotificationResponseDto>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var notification = await _notificationService.CreateNotificationAsync(createDto);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotificationId }, notification);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a notification
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<NotificationResponseDto>> UpdateNotification(int id, [FromBody] UpdateNotificationDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var notification = await _notificationService.UpdateNotificationAsync(id, updateDto);
                if (notification == null)
                    return NotFound(new { message = $"Notification with ID {id} was not found." });

                return Ok(notification);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(id);
                if (!result)
                    return NotFound(new { message = $"Notification with ID {id} was not found." });

                return Ok(new { message = "Notification marked as read." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking the notification as read.", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        [HttpPost("user/{userId}/read-all")]
        public async Task<ActionResult> MarkAllAsRead(int userId)
        {
            try
            {
                var result = await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { message = "All notifications marked as read." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking all notifications as read.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                if (!result)
                    return NotFound(new { message = $"Notification with ID {id} was not found." });

                return Ok(new { message = "Notification deleted successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the notification.", error = ex.Message });
            }
        }
    }
}
