using SmartTransportation.BLL.DTOs.Notification;

namespace SmartTransportation.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto?> GetNotificationByIdAsync(int notificationId);
        Task<IEnumerable<NotificationResponseDto>> GetAllNotificationsAsync();
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<NotificationResponseDto>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<NotificationResponseDto?> UpdateNotificationAsync(int notificationId, UpdateNotificationDto updateDto);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }
}

