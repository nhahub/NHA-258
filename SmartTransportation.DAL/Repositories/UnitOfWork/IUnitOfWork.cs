using System;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IBookingRepository Bookings { get; }
        IBookingPassengerRepository BookingPassengers { get; }
        IBookingSegmentRepository BookingSegments { get; }
        IMapLocationRepository MapLocations { get; }
        INotificationRepository Notifications { get; }
        IPaymentRepository Payments { get; }
        IRatingRepository Ratings { get; }
        IRouteRepository Routes { get; }
        IRouteSegmentRepository RouteSegments { get; }
        ITripRepository Trips { get; }
        ITripLocationRepository TripLocations { get; }
        IUserRepository Users { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserTypeRepository UserTypes { get; }
        IVehicleRepository Vehicles { get; }
        IWeatherRepository Weathers { get; }

        // Save changes
        Task<int> SaveAsync();
    }
}
