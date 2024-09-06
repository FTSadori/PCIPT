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
            float DAILY_TIME_FUND, int WORKING_DAYS, float MAX_DAILY_CARGO
            )
        {
            var totalMasses = CargoTurnoverCalculator.GetTotalMassForEachCargo(points);
            float fullMass = totalMasses.Sum(m => m.Value);

            Dictionary<PointFilesData, List<VehicleInRouteStats>> vehicleInRoutes = new();
            foreach (var point in points)
            {
                float distance = RouteCalculator.GetDistanceBetween(routes, point.SourceId, point.DestinationId);

                var cargo = cargoes.Find(c => c.Code == point.CargoCode);
                if (distance == float.PositiveInfinity || cargo == null)
                    continue;

                var rn = VehicleInRouteStatsCalculator.CalculateVehicleStats(vehicles, vehicleTypes, costTable, point, cargo,
                    distance, DAILY_TIME_FUND, fullMass, WORKING_DAYS, MAX_DAILY_CARGO).OrderBy(o => o.TotalCost).ToList();

                string filename = point.SourceId + "_" + point.DestinationId + "___" + cargo.Name + ".csv";
                vehicleInRoutes[new PointFilesData(point.Id, filename, distance, point.SourceId)] = rn;
            }
            return vehicleInRoutes;
        }

        public static List<VehicleByRoutesRow> DistributeTasksByVehicles(
            Dictionary<PointFilesData, List<VehicleInRouteStats>> vrstats,
            List<VehicleDto> vehicles,
            List<CargoTurnoverPointDto> points,
            List<RouteDto> routes,
            float dailyTimeFund
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
                float remainsTimeFraction = 1f;

                foreach (var vrrow in vrstat.Value)
                {
                    // рахуємо необхідну цілу кількість вільних циклів
                    int Cneeds = (int)MathF.Ceiling(vrrow.TotalNumberOfCycles * remainsTimeFraction);

                    // рахуємо максимальну кількість вільних циклів
                    int Creal = 0;

                    float v = vehicles.Find(v => v.Name == vrrow.Name)?.SpeedWithoutLoad ?? 0f;

                    for (int i = 0; i < BusyVehicles[vrrow.Name].Count; ++i)
                    {
                        var entry = BusyVehicles[vrrow.Name][i];
                        // рахуємо вільний час
                        entry.Trc = RouteCalculator.GetDistanceBetween(routes, entry.IdPointCurrent, vrstat.Key.SourceId) / v;
                        entry.Trs = RouteCalculator.GetDistanceBetween(routes, vrstat.Key.SourceId, entry.IdPointStart) / v;
                        if (entry.Trc == float.PositiveInfinity || entry.Trs == float.PositiveInfinity)
                            continue;

                        float tleft = dailyTimeFund - entry.UsedTime - entry.Trc - entry.Trs;
                        int cycles = (int)MathF.Floor(tleft / vrrow.TransportCycleSize);
                        Creal += cycles;
                    }

                    Creal += (MaxVehicles[vrrow.Name] - BusyVehicles[vrrow.Name].Count) * (int)vrrow.NumberOfTransportCyclesPerDay;
                    //distributionTable.Add(new VehicleByRoutesRow("Creal", Creal, 0, 0, 0, 0, 0, 0, 0));
                    
                    // скільки використаємо насправді
                    int Cmin = Math.Min(Creal, Cneeds);

                    float Tau = 0f;
                    List<int> UsedFromBusy = new();
                    Dictionary<int, int> CyclesFromBusy = new();
                    for (int i = 0; i < BusyVehicles[vrrow.Name].Count; ++i)
                    {
                        var entry = BusyVehicles[vrrow.Name][i];
                        float minPotentialTimeWaste = entry.UsedTime + entry.Trs + entry.Trc;

                        if (dailyTimeFund - minPotentialTimeWaste - vrrow.TransportCycleSize >= 0f)
                        {
                            Tau += minPotentialTimeWaste;
                            UsedFromBusy.Add(i);
                            CyclesFromBusy.Add(i, 0);
                        }
                    }
                  
                    UsedFromBusy = UsedFromBusy.OrderByDescending(i => BusyVehicles[vrrow.Name][i].UsedTime).ToList();

                    float Cr = Cmin;

                    float Ctotal = Tau / vrrow.TransportCycleSize + Cmin;
                    int Ntotal = (int)MathF.Ceiling(Ctotal / vrrow.NumberOfTransportCyclesPerDay);
                    float CAverage = Ctotal / Ntotal;
                    float TauAverage = CAverage * vrrow.TransportCycleSize;

                    List<int> NewUsedFromBusy = new();
                    foreach (var index in UsedFromBusy)
                    {
                        var entry = BusyVehicles[vrrow.Name][index];
                        if (entry.UsedTime + entry.Trc + entry.Trs + vrrow.TransportCycleSize >= TauAverage)
                        {
                            Ctotal -= (entry.UsedTime + entry.Trc + entry.Trs) / vrrow.TransportCycleSize;
                            Ntotal = (int)MathF.Ceiling(Ctotal / vrrow.NumberOfTransportCyclesPerDay);
                            CAverage = Ctotal / Ntotal;
                            TauAverage = CAverage * vrrow.TransportCycleSize;

                            CyclesFromBusy.Remove(index);
                            continue;
                        }
                        NewUsedFromBusy.Add(index);

                        float tleft = TauAverage - (entry.UsedTime + entry.Trc + entry.Trs);
                        int cleft = (int)MathF.Floor(tleft / vrrow.TransportCycleSize);
                        entry.UsedTime += vrrow.TransportCycleSize * cleft + entry.Trc;
                        CyclesFromBusy[index] += cleft;
                        Cr -= cleft;
                    }
                    //distributionTable.Add(new VehicleByRoutesRow("NewUsedFromBusyCount", NewUsedFromBusy.Count, Ntotal, 0, 0, 0, 0, 0, 0));

                    while (NewUsedFromBusy.Count < Ntotal)
                    {
                        float tleft = TauAverage;
                        int cleft = (int)MathF.Floor(tleft / vrrow.TransportCycleSize);
                        Cr -= cleft;

                        BusyVehicles[vrrow.Name].Add(new(vrrow.TransportCycleSize * cleft, vrstat.Key.SourceId, vrstat.Key.SourceId, 0f, 0f));
                        NewUsedFromBusy.Add(BusyVehicles[vrrow.Name].Count - 1);
                        CyclesFromBusy.Add(BusyVehicles[vrrow.Name].Count - 1, cleft);
                    }

                    // якщо все ще треба розподілити цикли
                    if (Cr > 0)
                    {
                        foreach (var index in NewUsedFromBusy)
                        {
                            var entry = BusyVehicles[vrrow.Name][index];
                            if (entry.UsedTime + entry.Trs + vrrow.TransportCycleSize <= dailyTimeFund)
                            {
                                entry.UsedTime += vrrow.TransportCycleSize;
                                CyclesFromBusy[index] += 1;
                                Cr -= 1;
                                if (Cr == 0)
                                    break;
                            }
                        }
                    }

                    // нарешті щось
                    foreach (var index in NewUsedFromBusy)
                    {
                        var entry = BusyVehicles[vrrow.Name][index];
                        entry.IdPointCurrent = vrstat.Key.SourceId;
                        float cyclesTimeUsed = CyclesFromBusy[index] * vrrow.TransportCycleSize;
                        float fractionUsed = cyclesTimeUsed / dailyTimeFund;
                        distributionTable.Add(new VehicleByRoutesRow(vrrow.Name, index + 1, vrstat.Key.Id, fractionUsed, 
                            vrstat.Key.Distance, points.Find(p => p.Id == vrstat.Key.Id)?.CargoCode ?? 0,
                            vrrow.TransportCycleSize, CyclesFromBusy[index], cyclesTimeUsed + entry.Trc));
                    }

                    float workFractionDone = Cmin / MathF.Ceiling(vrrow.TotalNumberOfCycles);
                    remainsTimeFraction -= workFractionDone;

                    //distributionTable.Add(new VehicleByRoutesRow("Point", 0, 0, 0, CyclesFromBusy.Count, Cmin, MathF.Ceiling(vrrow.TotalNumberOfCycles), workFractionDone, remainsTimeFraction));

                    if (remainsTimeFraction < 0.001f)
                        break;
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
