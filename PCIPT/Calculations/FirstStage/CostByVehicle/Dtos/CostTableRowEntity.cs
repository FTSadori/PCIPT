using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.CostByVehicle.Dtos
{
    public sealed record CostTableRowEntity(string Name, float HydraulicOilConsumption, float TransmissionOilConsumption, float RepairCosts, float BaseElectricityConsumption, float MotorOilConsumption, float FuelConsumption)
    {
        public float TotalLoss
        {
            get
            {
                return HydraulicOilConsumption + TransmissionOilConsumption + RepairCosts + BaseElectricityConsumption + FuelConsumption + MotorOilConsumption;
            }
        }
    };
}
