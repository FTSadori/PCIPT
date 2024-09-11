using PCIPT.Calculations.FirstStage.DefineRoutes;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Calculations.FirstStage.VehicleInRoute;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCIPT.Calculations.FirstStage.FinalCostByVehicle
{
    public sealed class FinalCostCalculator
    {
        static public List<FinalCostRowEntity> CalculateFinalCost(
            List<VehicleByRoutesRow> vehiclesByRoutes,
            List<FuelVehicleDto> fuelVehicles,
            List<ElectricVehicleDto> electricVehicles,
            float dailyTimeFund
            )
        {
            List<FinalCostRowEntity> finalCostRows = new();

            Dictionary<string, float> vehiclesTotalFraction = new();
            Dictionary<string, int> vehiclesCount = new();
            float[] totalStats = new float[9];

            foreach (var vehicle in vehiclesByRoutes)
            {
                if (vehiclesCount.ContainsKey(vehicle.Name))
                {
                    vehiclesCount[vehicle.Name] = Math.Max(vehiclesCount[vehicle.Name], vehicle.Number);
                    vehiclesTotalFraction[vehicle.Name] += vehicle.FractionOfTimeUsed;
                }
                else 
                {
                    vehiclesCount[vehicle.Name] = 1;
                    vehiclesTotalFraction[vehicle.Name] = vehicle.FractionOfTimeUsed;
                }
            }

            foreach (var vtFraction in vehiclesTotalFraction)
            {
                string name = vtFraction.Key;
                float f = vtFraction.Value;

                var fueldto = fuelVehicles.Find(v => v.Name == name);
                if (fueldto != null)
                {
                    finalCostRows.Add(new FinalCostRowEntity(name, vehiclesCount[name], f, f * dailyTimeFund,
                        f * fueldto.HydraulicOilConsumption, f * fueldto.TransmissionOilConsumption, f * fueldto.SpecialOilConsumption,
                        0f, f * fueldto.MotorOilConsumption, f * fueldto.FuelConsumption));
                    continue;
                }

                var elecdto = electricVehicles.Find(v => v.Name == name);
                if (elecdto != null)
                {
                    finalCostRows.Add(new FinalCostRowEntity(name, vehiclesCount[name], f, f * dailyTimeFund,
                        f * elecdto.HydraulicOilConsumption, f * elecdto.TransmissionOilConsumption, f * elecdto.SpecialOilConsumption,
                        f * elecdto.BaseElectricityConsumption, 0f, 0f));
                }
            }

            foreach (var v in finalCostRows) 
            {
                totalStats[0] += v.TotalNumber;
                totalStats[1] += v.TotalFraction;
                totalStats[2] += v.TotalTime;
                totalStats[3] += v.HydraulicOilConsumption;
                totalStats[4] += v.TransmissionOilConsumption;
                totalStats[5] += v.SpecialOilConsumption;
                totalStats[6] += v.BaseElectricityConsumption;
                totalStats[7] += v.MotorOilConsumption;
                totalStats[8] += v.FuelConsumption;
            }

            finalCostRows.Add(new FinalCostRowEntity("Total", (int)totalStats[0], totalStats[1], totalStats[2],
                totalStats[3], totalStats[4], totalStats[5],
                totalStats[6], totalStats[7], totalStats[8]));

            return finalCostRows;
        }
    }
}
