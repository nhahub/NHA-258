// Path: SmartTransportation.BLL/Interfaces/IOpenWeatherGateway.cs
using SmartTransportation.DAL.Models;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.Interfaces
{
    public interface IOpenWeatherGateway
    {
        Task<Weather> FetchWeatherDataAsync(int routeId, decimal lat, decimal lon);
    }
}