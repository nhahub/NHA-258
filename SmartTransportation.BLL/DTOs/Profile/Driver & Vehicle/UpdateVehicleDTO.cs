using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Profile
{
    public class UpdateVehicleDTO
    {
       
        public int VehicleId { get; set; }
        

        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleYear { get; set; }

        public string? PlateNumber { get; set; }
        public string? Color { get; set; }
        public int   SeatsCount { get; set; }

        public string? VehicleLicenseNumber { get; set; }
        public DateOnly? VehicleLicenseExpiry { get; set; }
    }

}
