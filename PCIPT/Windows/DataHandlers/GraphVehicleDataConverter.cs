using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Windows.ObjectCreators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PCIPT.Windows.DataHandlers
{
    public record GraphVehiclesData(string Type, NegSize Coord);

    public sealed class GraphVehicleDataConverter
    {
        static Random random = new();
        const double RandomMaxRange = 0.1;

        public static List<GraphVehiclesData> InitConvert(List<VehicleByRoutesRow> vehicleByRoutes, Dictionary<int, NegSize> nodesCoords)
        {
            List<GraphVehiclesData> result = new();

            Dictionary<string, List<NegSize>> typesAndStartPoints = new();
            foreach (var vehicleByRoute in vehicleByRoutes)
            {
                if (!typesAndStartPoints.ContainsKey(vehicleByRoute.Name))
                {
                    typesAndStartPoints.Add(vehicleByRoute.Name, new());
                }
                if (typesAndStartPoints[vehicleByRoute.Name].Count < vehicleByRoute.Number)
                {
                    double randomAngle = random.NextDouble() * 360.0;
                    double randomLength = random.NextDouble() * RandomMaxRange;
                    NegSize randomizedCoords = new(
                        nodesCoords[vehicleByRoute.PointId].Width + randomLength * Math.Cos(randomAngle),
                        nodesCoords[vehicleByRoute.PointId].Height + randomLength * Math.Sin(randomAngle));
                    typesAndStartPoints[vehicleByRoute.Name].Add(randomizedCoords);
                }
            }

            foreach (var type in typesAndStartPoints)
            {
                foreach (var vehicle in type.Value)
                {
                    result.Add(new(type.Key, vehicle));
                }
            }

            return result;
        }

        public static List<GraphVehiclesData> ConvertToScreenValues(List<GraphVehiclesData> vehiclesCoords, Canvas VehiclesCanvas, double size)
        {
            double NormalDistance = Math.Min(VehiclesCanvas.ActualWidth, VehiclesCanvas.ActualHeight) / 2.0 * 0.8 * size;

            NegSize zeroPoint = new(VehiclesCanvas.ActualWidth / 2 * size, VehiclesCanvas.ActualHeight / 2 * size);

            List<GraphVehiclesData> result = new();
            foreach (var vehicle in vehiclesCoords)
            {
                result.Add(new(vehicle.Type, new(
                    zeroPoint.Width + NormalDistance * vehicle.Coord.Width,
                    zeroPoint.Height + NormalDistance * vehicle.Coord.Height)));
            }
            return result;
        }
    }
}
