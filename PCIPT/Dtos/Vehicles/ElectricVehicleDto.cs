using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Vehicles
{
    public sealed record ElectricVehicleDto
        (string Name = "", string Type = "", float LoadCapacity = 0, float SpeedWithLoad = 0, float SpeedWithoutLoad = 0, float LoadTime = 0,
        float HydraulicOilConsumption = 0, float TransmissionOilConsumption = 0, float SpecialOilConsumption = 0, int MaxQuantity = 0,
        float BaseElectricityConsumption = 0)
        : VehicleDto(Name, Type, LoadCapacity, SpeedWithLoad, SpeedWithoutLoad, LoadTime, HydraulicOilConsumption, TransmissionOilConsumption, SpecialOilConsumption, MaxQuantity);
}