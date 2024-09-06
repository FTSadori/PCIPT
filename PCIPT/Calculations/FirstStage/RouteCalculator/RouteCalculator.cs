using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.RouteFinder
{
    public sealed class RouteCalculator
    {
        public static float GetDistanceBetween(List<RouteDto> routes, int startPointId, int endPointId)
        {
            string var1 = startPointId + "_" + endPointId;
            string var2 = endPointId + "_" + startPointId;

            if (routesData.ContainsKey(var1))
                return routesData[var1];
            if (routesData.ContainsKey(var2))
                return routesData[var2];
            

            
            return float.PositiveInfinity;
        }

        public static void InitRoutes(List<RouteDto> routes)
        {
            routesData.Clear();
            foreach (var route in routes)
            {
                routesData.Add(route.SourceId + "_" + route.DestinationId, route.Distance);
            }
        }

        private sealed record Path(List<int> Nodes, double TotalDistance);

        private static Dictionary<string, float> routesData = new();
        private static Dictionary<string, Path> savedRoutes = new();
    }
}
