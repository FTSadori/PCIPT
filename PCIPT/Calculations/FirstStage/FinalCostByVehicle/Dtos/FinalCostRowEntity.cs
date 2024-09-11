using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.FinalCostByVehicle.Dtos
{
    public sealed record FinalCostRowEntity(
        string Name, 
        int TotalNumber, 
        float TotalFraction,
        float TotalTime,
        float HydraulicOilConsumption,
        float TransmissionOilConsumption, 
        float SpecialOilConsumption, 
        float BaseElectricityConsumption, 
        float MotorOilConsumption,
        float FuelConsumption
        );
}
