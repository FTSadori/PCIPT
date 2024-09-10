using CsvHelper.Configuration.Attributes;
using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.VehicleInRoute.Dtos;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace PCIPT.Calculations.FirstStage.VehicleInRoute
{
    public sealed class VehicleInRouteStatsCalculator
    {
        private static float CalculateDailyCargoTurnover(float thisCargoTurnover, float fullCargoTurnover, float workingDays, float maxDailyCargo)
        {
            float Qav = fullCargoTurnover / workingDays;
            float K = maxDailyCargo / Qav;
            float Qdaily = thisCargoTurnover / workingDays * K;
            return Qdaily;
        }

        public static List<VehicleInRouteStats> CalculateVehicleStats(List<VehicleDto> vehicles, List<VehicleTypeDto> vehicleTypes, 
            List<CostTableRowEntity> costTable,
            CargoTurnoverPointDto point, CargoDto cargo, float distance,
            float dailyTimeFund, float fullCargoTurnover, float workingDays, float maxDailyCargo)
        {
            float Qdaily = CalculateDailyCargoTurnover(point.OutgoingCargo, fullCargoTurnover, workingDays, maxDailyCargo);

            var vehicleTable = new List<VehicleInRouteStats>();

            foreach (var vehicle in vehicles)
            {
                var types = vehicleTypes.Find(t => t.VehicleType == vehicle.Type && t.CargoType == cargo.Type);
                if (types == null) 
                    continue;

                float Tc1 = distance * 60f / (vehicle.SpeedWithLoad * 1000f) + distance * 60f / (vehicle.SpeedWithoutLoad * 1000f) + 2f * vehicle.LoadTime;
                //float Tc2 = 2f * (distance * 60f / (vehicle.SpeedWithLoad * 1000f)) + 4f * vehicle.LoadTime;
                float cdaily = MathF.Floor(dailyTimeFund / Tc1);
                float q = vehicle.LoadCapacity * cargo.CapacityUtilisationRate;
                float qdaily = q * cdaily;
                float N = Qdaily / qdaily;

                float cost = costTable.Find(c => c.Name == vehicle.Name)?.TotalLoss ?? -1f;
                if (cost < 0.0001f)
                    continue;

                float mtotal = cdaily * N;
                float ttotal = Tc1 * mtotal;

                vehicleTable.Add(new VehicleInRouteStats(vehicle.Name, Tc1, cdaily, q, qdaily, N, N * cost, mtotal, ttotal));
            }

            return vehicleTable;
        }
    }
}
