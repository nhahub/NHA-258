using SmartTransportation.DAL.Models;
using System.Threading.Tasks;

namespace SmartTransportation.DAL.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TransportationContext _context;

        public UnitOfWork(TransportationContext context)
        {
            _context = context;

            // Initialize repositories
            Bookings = new BookingRepository(_context);
            BookingPassengers = new BookingPassengerRepository(_context);
            BookingSegments = new BookingSegmentRepository(_context);
            MapLocations = new MapLocationRepository(_context);
            Notifications = new NotificationRepository(_context);
            Payments = new PaymentRepository(_context);
            Ratings = new RatingRepository(_context);
            Routes = new RouteRepository(_context);
            RouteSegments = new RouteSegmentRepository(_context);
            Trips = new TripRepository(_context);
            TripLocations = new TripLocationRepository(_context);
            Users = new UserRepository(_context);
            UserProfiles = new UserProfileRepository(_context);
            UserTypes = new UserTypeRepository(_context);
            Vehicles = new VehicleRepository(_context);
            Weathers = new WeatherRepository(_context);
        }

        // Repositories
        public IBookingRepository Bookings { get; private set; }
        public IBookingPassengerRepository BookingPassengers { get; private set; }
        public IBookingSegmentRepository BookingSegments { get; private set; }
        public IMapLocationRepository MapLocations { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public IRatingRepository Ratings { get; private set; }
        public IRouteRepository Routes { get; private set; }
        public IRouteSegmentRepository RouteSegments { get; private set; }
        public ITripRepository Trips { get; private set; }
        public ITripLocationRepository TripLocations { get; private set; }
        public IUserRepository Users { get; private set; }
        public IUserProfileRepository UserProfiles { get; private set; }
        public IUserTypeRepository UserTypes { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IWeatherRepository Weathers { get; private set; }

        // Save changes
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Dispose
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
