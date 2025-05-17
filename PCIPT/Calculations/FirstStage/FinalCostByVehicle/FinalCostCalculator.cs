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
            float[] totalStats = new float[10];

            Dictionary<string, int> vehiclesUsedCount = new();

            foreach (var vehicle in vehiclesByRoutes)
            {
                if (vehiclesCount.ContainsKey(vehicle.Name))
                {
                    vehiclesCount[vehicle.Name] = Math.Max(vehiclesCount[vehicle.Name], vehicle.Number);
                    if (vehicle.CargoCode != -1)
                    {
                        vehiclesUsedCount[vehicle.Name] = Math.Max(vehiclesUsedCount[vehicle.Name], vehicle.Number);
                    }
                    vehiclesTotalFraction[vehicle.Name] += vehicle.FractionOfTimeUsed;
                }
                else 
                {
                    vehiclesCount[vehicle.Name] = 1;
                    if (vehicle.CargoCode != -1)
                    {
                        vehiclesUsedCount[vehicle.Name] = 1;
                    }
                    else vehiclesUsedCount[vehicle.Name] = 0; 
                    vehiclesTotalFraction[vehicle.Name] = vehicle.FractionOfTimeUsed;
                }
            }

            foreach (var vtFraction in vehiclesTotalFraction)
            {
                string name = vtFraction.Key;
                float f = vtFraction.Value / 60 * dailyTimeFund;

                var fueldto = fuelVehicles.Find(v => v.Name == name);
                if (fueldto != null)
                {
                    finalCostRows.Add(new FinalCostRowEntity(name, vehiclesUsedCount[name], vehiclesCount[name], f, f * dailyTimeFund,
                        f * fueldto.HydraulicOilConsumption, f * fueldto.TransmissionOilConsumption, f * fueldto.RepairCosts,
                        0f, f * fueldto.MotorOilConsumption, f * fueldto.FuelConsumption));
                    continue;
                }

                var elecdto = electricVehicles.Find(v => v.Name == name);
                if (elecdto != null)
                {
                    finalCostRows.Add(new FinalCostRowEntity(name, vehiclesUsedCount[name], vehiclesCount[name], f, f * dailyTimeFund,
                        f * elecdto.HydraulicOilConsumption, f * elecdto.TransmissionOilConsumption, f * elecdto.RepairCosts,
                        f * elecdto.BaseElectricityConsumption, 0f, 0f));
                }
            }

            foreach (var v in finalCostRows) 
            {
                totalStats[0] += v.UsedNumber;
                totalStats[1] += v.TotalNumber;
                totalStats[2] += v.TotalFraction;
                totalStats[3] += v.TotalTime;
                totalStats[4] += v.HydraulicOilConsumption;
                totalStats[5] += v.TransmissionOilConsumption;
                totalStats[6] += v.RepairCosts;
                totalStats[7] += v.BaseElectricityConsumption;
                totalStats[8] += v.MotorOilConsumption;
                totalStats[9] += v.FuelConsumption;
            }

            finalCostRows.Add(new FinalCostRowEntity("Total", (int)totalStats[0], (int)totalStats[1], totalStats[2], totalStats[3],
                totalStats[4], totalStats[5], totalStats[6],
                totalStats[7], totalStats[8], totalStats[9]));

            return finalCostRows;
        }
    }
}
