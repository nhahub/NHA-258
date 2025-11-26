using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Profile
{
    public class UpdateDriverProfileDTO : BaseUserProfileDTO
    {
        public string? DriverLicenseNumber { get; set; }
        public DateOnly? DriverLicenseExpiry { get; set; }
    }

}
