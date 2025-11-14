using AutoMapper;
using SmartTransportation.BLL.DTOs.Location;
using SmartTransportation.BLL.DTOs.Route;
using SmartTransportation.BLL.DTOs.Trip;
using SmartTransportation.BLL.DTOs.Weather;
using SmartTransportation.DAL.Models;

namespace SmartTransportation.BLL.Utilities
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DAL.Models.Route, RouteDetailsDTO>()
                .ForMember(dest => dest.LatestWeather, opt => opt.MapFrom(src => 
                    src.Weathers.OrderByDescending(w => w.WeatherDate).FirstOrDefault()));

            CreateMap<CreateRouteDTO, DAL.Models.Route>();

            CreateMap<DAL.Models.MapLocation, MapLocationDTO>();
            CreateMap<MapLocationDTO, DAL.Models.MapLocation>();

            CreateMap<DAL.Models.RouteSegment, RouteSegmentDTO>();
            CreateMap<CreateSegmentDTO, DAL.Models.RouteSegment>();

            CreateMap<DAL.Models.Weather, WeatherDTO>();
            CreateMap<WeatherDTO, DAL.Models.Weather>();

            CreateMap<DAL.Models.Trip, TripDetailsDTO>();
            CreateMap<CreateTripDTO, DAL.Models.Trip>();
        }
    }
}