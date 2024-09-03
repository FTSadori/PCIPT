using PCIPT.Calculations.FirstStage.CargoStats;
using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Calculations.FirstStage.VehicleInRoute;
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

namespace PCIPT.Calculations.FirstStage.DefineRoutes
{
    public sealed class VehiclesByRoutesCalculator
    {
        public static Dictionary<RouteFilesData, List<VehicleInRouteStats>> CalculateVehicleStatsByPoints(
            List<CargoTurnoverPointDto> points, List<RouteDto> routes, List<CargoDto> cargoes, List<VehicleDto> vehicles,
            List<VehicleTypeDto> vehicleTypes, List<CostTableRowEntity> costTable,
            float DAILY_TIME_FUND, int WORKING_DAYS, float MAX_DAILY_CARGO
            )
        {
            var totalMasses = CargoTurnoverCalculator.GetTotalMassForEachCargo(points);
            float fullMass = totalMasses.Sum(m => m.Value);

            Dictionary<RouteFilesData, List<VehicleInRouteStats>> vehicleInRoutes = new();
            foreach (var point in points)
            {
                var route = routes.Find(r => r.SourceName == point.SourceName && r.DestinationName == point.DestinationName);
                var cargo = cargoes.Find(c => c.Code == point.CargoCode);
                if (route == null || cargo == null)
                    continue;

                float distance = route.Distance; // todo calculate distance

                var rn = VehicleInRouteStatsCalculator.CalculateVehicleStats(vehicles, vehicleTypes, costTable, point, cargo,
                    distance, DAILY_TIME_FUND, fullMass, WORKING_DAYS, MAX_DAILY_CARGO).OrderBy(o => o.TotalCost).ToList();

                string filename = route.SourceName + "_" + route.DestinationName + "___" + cargo.Name + ".csv";
                vehicleInRoutes[new RouteFilesData(point.Id, filename, distance)] = rn;
            }
            return vehicleInRoutes;
        }

        record BusyVehicleData(int Number, float RemainsTime);

        public static List<VehicleByRoutesRow> DistributeTasksByVehicles(
            Dictionary<RouteFilesData, List<VehicleInRouteStats>> vrstats, 
            List<VehicleDto> vehicles,
            List<CargoTurnoverPointDto> points
            )
        {
            List<VehicleByRoutesRow> distributionTable = new();

            Dictionary<string, float> remainsVehicles = new();
            Dictionary<string, int> currentNumber = new();
            Dictionary<string, List<BusyVehicleData>> currentVehicles = new();

            foreach (var vehicle in vehicles)
            {
                remainsVehicles.Add(vehicle.Name, vehicle.MaxQuantity);
                currentVehicles.Add(vehicle.Name, new());
                currentNumber.Add(vehicle.Name, 0);
            }

            foreach (var vrstat in vrstats)
            {
                float remainsTime = 1f;
                var route = points.Find(p => p.Id == vrstat.Key.PointId);
                if (route == null)
                    continue;

                float totalTimeFraction = 0f;
                List<VehicleInRouteStats> usedInRoute = new();

                foreach (var vrrow in vrstat.Value)
                {
                    while (remainsTime > 0.001f && remainsVehicles[vrrow.Name] > 0.001f)
                    {
                        float needsVehicles = vrrow.TheRequiredNumberOfVehicles * remainsTime;

                        float fraction = remainsVehicles[vrrow.Name] - MathF.Truncate(remainsVehicles[vrrow.Name]);
                        if (fraction < 0.001f)
                        {
                            fraction = 1f;
                        }

                        float delta = MathF.Min(fraction, needsVehicles);
                        totalTimeFraction += delta;
                        usedInRoute.Add(vrrow);

                        var tnoc = (int)Math.Ceiling(vrrow.NumberOfTransportCyclesPerDay * delta);

                        remainsVehicles[vrrow.Name] -= delta;
                        remainsTime -= delta / vrrow.TheRequiredNumberOfVehicles;
                    }
                }

                Dictionary<string, List<BusyVehicleData>> newCurrentVehicles = new();
                foreach (var vehicle in vehicles) newCurrentVehicles.Add(vehicle.Name, new());

                int left = 0;
                float eachTimeFraction = totalTimeFraction / usedInRoute.Count;
                foreach (var vrrow in usedInRoute)
                {
                    int number;
                    float spent;
                    if (currentVehicles[vrrow.Name].Count == 0)
                    {
                        currentNumber[vrrow.Name] += 1;
                        number = currentNumber[vrrow.Name];
                        spent = eachTimeFraction;
                        newCurrentVehicles[vrrow.Name].Add(new BusyVehicleData(currentNumber[vrrow.Name], 1f - eachTimeFraction));
                    }
                    else
                    {
                        var save = currentVehicles[vrrow.Name].First();
                        number = save.Number;
                        if (save.RemainsTime > eachTimeFraction)
                        {
                            spent = eachTimeFraction;
                            newCurrentVehicles[vrrow.Name].Add(new BusyVehicleData(save.Number, save.RemainsTime - eachTimeFraction));
                        }
                        else
                        {
                            spent = save.RemainsTime;
                            totalTimeFraction -= save.RemainsTime;
                            left += 1;
                            eachTimeFraction = totalTimeFraction / (usedInRoute.Count - left);
                        }
                        currentVehicles[vrrow.Name].Remove(save);
                    }

                    var tnoc = (int)Math.Ceiling(vrrow.NumberOfTransportCyclesPerDay * spent);

                    distributionTable.Add(new VehicleByRoutesRow(
                            vrrow.Name, number, route.Id, spent, vrstat.Key.Distance, route.CargoCode,
                            vrrow.TransportCycleSize, tnoc, vrrow.TransportCycleSize * tnoc));
                }
                currentVehicles = newCurrentVehicles;
            }

            return distributionTable;
        }
    }
}
