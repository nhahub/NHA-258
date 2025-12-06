using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Route
{
    public class UpdateRouteDTO:CreateRouteDTO
    {
        public int RouteId { get; set; }
    }
}
