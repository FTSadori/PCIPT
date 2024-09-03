using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Vehicles
{
    public sealed record FuelVehicleDto
        (string Name, string Type, float LoadCapacity, float SpeedWithLoad, float SpeedWithoutLoad, float LoadTime,
        float HydraulicOilConsumption, float TransmissionOilConsumption, float SpecialOilConsumption, int MaxQuantity,
        string FuelType, float FuelConsumption, float MotorOilConsumption)
        : VehicleDto(Name, Type, LoadCapacity, SpeedWithLoad, SpeedWithoutLoad, LoadTime, HydraulicOilConsumption, TransmissionOilConsumption, SpecialOilConsumption, MaxQuantity);
}
