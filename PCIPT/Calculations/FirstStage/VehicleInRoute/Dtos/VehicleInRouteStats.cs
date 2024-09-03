using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.VehicleInRoute.Dtos
{
    public sealed record VehicleInRouteStats(string Name,
        float TransportCycleSize,
        float NumberOfTransportCyclesPerDay,
        float RoutePerformance,
        float AverageDailyCargoVolume,
        float TheRequiredNumberOfVehicles,
        float TotalCost,
        float TotalNumberOfCycles,
        float TotalTimeDailyNeeded);
}
