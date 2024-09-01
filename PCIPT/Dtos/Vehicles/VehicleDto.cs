using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Vehicles
{
    public record VehicleDto
        (string Name, string Type, float LoadCapacity, float SpeedWithLoad, float SpeedWithoutLoad, float LoadTime, 
        float HydraulicOilConsumption, float TransmissionOilConsumption, float SpecialOilConsumption)
        : IData;
}
