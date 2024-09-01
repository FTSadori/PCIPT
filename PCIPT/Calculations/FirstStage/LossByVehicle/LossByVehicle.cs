using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.LossByVehicle
{
    public sealed class LossByVehicle
    {
        public string Name = "";
        public float HydraulicOilConsumption;
        public float TransmissionOilConsumption;
        public float SpecialOilConsumption;
        public float BaseElectricityConsumption;
        public float FuelConsumption;
        public float MotorOilConsumption;
        public float TotalLoss { get 
            { 
                return HydraulicOilConsumption + TransmissionOilConsumption + SpecialOilConsumption + BaseElectricityConsumption + FuelConsumption + MotorOilConsumption; 
            }
        }
    };
}
