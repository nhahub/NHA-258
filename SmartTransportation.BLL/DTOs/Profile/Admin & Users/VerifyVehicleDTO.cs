using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Profile
{
    public class VerifyVehicleDTO
    {
       
        public int DriverId { get; set; }  // <-- added
        public bool IsVerified { get; set; }
    }

}
