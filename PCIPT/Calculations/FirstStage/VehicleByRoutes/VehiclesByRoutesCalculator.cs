using CsvHelper.Configuration.Attributes;
using PCIPT.Calculations.FirstStage.CargoStats;
using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Calculations.FirstStage.VehicleInRoute;
using PCIPT.Calculations.FirstStage.VehicleInRoute.Dtos;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;

namespace PCIPT.Calculations.FirstStage.DefineRoutes
{
    public sealed class VehiclesByRoutesCalculator
    {
        public static Dictionary<PointFilesData, List<VehicleInRouteStats>> CalculateVehicleStatsByPoints(
            List<CargoTurnoverPointDto> points, List<RouteDto> routes, List<CargoDto> cargoes, List<VehicleDto> vehicles,
            List<VehicleTypeDto> vehicleTypes, List<CostTableRowEntity> costTable,
            float DAILY_TIME_FUND, int WORKING_DAYS
            )
        {
            var totalMasses = CargoTurnoverCalculator.GetTotalMassForEachCargo(points);
            float fullMass = totalMasses.Sum(m => m.Value);

            Dictionary<PointFilesData, List<VehicleInRouteStats>> vehicleInRoutes = new();
            foreach (var point in points)
            {
                float distance = RouteCalculator.GetDistanceBetween(point.SourceId, point.DestinationId);

                var cargo = cargoes.Find(c => c.Code == point.CargoCode);
                if (distance == float.PositiveInfinity || cargo == null)
                    continue;

                var rn = VehicleInRouteStatsCalculator.CalculateVehicleStats(vehicles, vehicleTypes, costTable, point, cargo,
                    distance, DAILY_TIME_FUND, fullMass, WORKING_DAYS).OrderBy(o => o.TotalCost).ToList();

                string filename = point.SourceId + "_" + point.DestinationId + "___" + cargo.Name + ".csv";
                vehicleInRoutes[new PointFilesData(point.Id, filename, distance, point.SourceId, point.DestinationId)] = rn;
            }
            return vehicleInRoutes;
        }

        public static List<VehicleByRoutesRow> DistributeTasksByVehicles(
            Dictionary<PointFilesData, List<VehicleInRouteStats>> vrstats,
            List<VehicleDto> vehicles,
            List<CargoTurnoverPointDto> points,
            List<RouteDto> routes,
            float dailyTimeFund,
            int workingDays
            )
        {
            List<VehicleByRoutesRow> distributionTable = new();

            Dictionary<string, List<ShortVehicleData>> BusyVehicles = new();
            Dictionary<string, int> MaxVehicles = new();
            foreach (var vehicle in vehicles)
            {
                BusyVehicles.Add(vehicle.Name, new());
                MaxVehicles.Add(vehicle.Name, vehicle.MaxQuantity);
            }

            foreach (var vrstat in vrstats)
            {
                float Tneeds = points.Find(p => p.Id == vrstat.Key.Id).OutgoingCargo / workingDays;
                float Treal = 0;

                // recalculate pathes
                foreach (var vrrow in vrstat.Value)
                {
                    float v = vehicles.Find(v => v.Name == vrrow.Name)?.SpeedWithoutLoad ?? 0f;
                    v = v * 1000f / 60f;

                    for (int i = 0; i < BusyVehicles[vrrow.Name].Count; ++i)
                    {
                        // конкретний ТЗ
                        var entry = BusyVehicles[vrrow.Name][i];
                        // рахуємо час переїзду
                        entry.Trc = RouteCalculator.GetDistanceBetween(entry.IdPointCurrent, vrstat.Key.SourceId) / v;
                        entry.Trs = RouteCalculator.GetDistanceBetween(vrstat.Key.DestinationId, entry.IdPointStart) / v;
                        if (entry.Trc == float.PositiveInfinity || entry.Trs == float.PositiveInfinity)
                            continue;

                        // рахуємо вільний час
                        float tleft = dailyTimeFund - entry.UsedTime - entry.Trc - entry.Trs;
                        // знаходимо цілу кількість циклів
                        int cycles = (int)MathF.Floor(tleft / vrrow.TransportCycleSize);
                        Treal += cycles * vrrow.RoutePerformance;
                    }
                }

                // if need to get new machine
                if (Treal < Tneeds)
                {
                    foreach (var vrrow in vrstat.Value)
                    {
                        var vehicle = vehicles.Find(v => v.Name == vrrow.Name);
                        // add new
                        while (BusyVehicles[vrrow.Name].Count < vehicle.MaxQuantity && Treal < Tneeds)
                        {
                            // current and start are equal
                            BusyVehicles[vrrow.Name].Add(new(0f, vrstat.Key.SourceId, vrstat.Key.SourceId, 0f, 0f));
                            Treal += vrrow.RoutePerformance * vrrow.NumberOfTransportCyclesPerDay;
                        }
                    }
                    if (Treal < Tneeds)
                    {
                        throw new Exception($"You can't fill cargo point {vrstat.Key.Id}");
                    }
                }

                // now we are cooking
                Dictionary<string, List<float>> freeCargoVolume = new();
                float maxFreeCargo = 0;
                float sumCargo = 0;
                foreach (var line in BusyVehicles)
                {
                    var row = vrstat.Value.Find(v => v.Name == line.Key);
                    if (row == null) continue;
                    freeCargoVolume[line.Key] = new();
                    for (int i = 0; i < line.Value.Count; ++i)
                    {
                        var freeTime = dailyTimeFund - line.Value[i].UsedTime - line.Value[i].Trs - line.Value[i].Trc;
                        freeCargoVolume[line.Key].Add(MathF.Floor(freeTime / dailyTimeFund * row.NumberOfTransportCyclesPerDay) * row.RoutePerformance);
                        maxFreeCargo = Math.Max(maxFreeCargo, freeCargoVolume[line.Key][i]);
                        sumCargo += freeCargoVolume[line.Key][i];
                    }
                }
                sumCargo /= maxFreeCargo;
                var TneedsFraction = Tneeds / sumCargo;
                float Tmore = 0;
                float Tcompleted = 0;

                // calculate and save
                foreach (var pair in freeCargoVolume)
                {
                    if (Tcompleted >= Tneeds) break;
                    var row = vrstat.Value.Find(v => v.Name == pair.Key);
                    if (row == null) continue;
                    for (int i = 0; i < pair.Value.Count; ++i) 
                    {
                        float Tvehicle = pair.Value[i] / maxFreeCargo * TneedsFraction - Tmore;
                        if (Tvehicle <= 0) continue;
                        int cycles = (int)MathF.Ceiling(Tvehicle / row.RoutePerformance);
                        Tcompleted += cycles * row.RoutePerformance;
                        Tmore += cycles * row.RoutePerformance - Tvehicle;
                        BusyVehicles[pair.Key][i].UsedTime += cycles * row.TransportCycleSize;
                        BusyVehicles[pair.Key][i].IdPointCurrent = vrstat.Key.DestinationId;

                        float fractionUsed = cycles * row.TransportCycleSize / dailyTimeFund;
                        distributionTable.Add(new VehicleByRoutesRow(pair.Key, i + 1, vrstat.Key.Id, fractionUsed,
                            vrstat.Key.Distance, points.Find(p => p.Id == vrstat.Key.Id)?.CargoCode ?? 0,
                            row.TransportCycleSize, cycles, cycles * row.TransportCycleSize + BusyVehicles[pair.Key][i].Trc));
                    }
                }

                foreach (var pair in BusyVehicles)
                {
                    pair.Value.RemoveAll(m => m.UsedTime < 0.1);
                }

            }

            foreach (var pair in MaxVehicles)
            {
                while (pair.Value > BusyVehicles[pair.Key].Count)
                {
                    BusyVehicles[pair.Key].Add(new(0, 1, 1, 0, 0));
                    distributionTable.Add(new(pair.Key, BusyVehicles[pair.Key].Count, 1, 0, 0, -1, 0, 0, 0));
                }
            }

            return distributionTable;
        }

        class ShortVehicleData
        {
            public float UsedTime;
            public int IdPointCurrent;
            public int IdPointStart;
            public float Trc;
            public float Trs;

            public ShortVehicleData(float usedTime, int idPointCurrent, int idPointStart, float trc, float trs)
            {
                UsedTime = usedTime;
                IdPointCurrent = idPointCurrent;
                IdPointStart = idPointStart;
                Trc = trc;
                Trs = trs;
            }
        }

    }
}
