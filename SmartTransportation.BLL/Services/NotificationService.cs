using SmartTransportation.BLL.DTOs.Notification;
using SmartTransportation.BLL.Exceptions;
using SmartTransportation.BLL.Interfaces;
using SmartTransportation.DAL.Models;
using SmartTransportation.DAL.Repositories.UnitOfWork;

namespace SmartTransportation.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NotificationResponseDto?> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdWithDetailsAsync(notificationId);
            if (notification == null) return null;

            return MapToResponseDto(notification);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync()
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync();
            return notifications.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            return notifications.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            return notifications.Where(n => !n.IsRead).Select(MapToResponseDto);
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(createDto.UserId);
            if (user == null)
                throw new NotFoundException("User", createDto.UserId);

            var notification = new Notification
            {
                UserId = createDto.UserId,
                Title = createDto.Title,
                Message = createDto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            var createdNotification = await _unitOfWork.Notifications.GetByIdWithDetailsAsync(notification.NotificationId);
            return MapToResponseDto(createdNotification!);
        }

        public async Task<NotificationResponseDto?> UpdateNotificationAsync(int notificationId, UpdateNotificationDto updateDto)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                throw new NotFoundException("Notification", notificationId);

            if (!string.IsNullOrEmpty(updateDto.Title))
                notification.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Message))
                notification.Message = updateDto.Message;

            if (updateDto.IsRead.HasValue)
                notification.IsRead = updateDto.IsRead.Value;

            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveAsync();

            var updatedNotification = await _unitOfWork.Notifications.GetByIdWithDetailsAsync(notificationId);
            return MapToResponseDto(updatedNotification!);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                throw new NotFoundException("Notification", notificationId);

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                throw new NotFoundException("Notification", notificationId);

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        private NotificationResponseDto MapToResponseDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                UserName = notification.User?.UserName ?? string.Empty,
                Title = notification.Title ?? string.Empty,
                Message = notification.Message ?? string.Empty,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}

