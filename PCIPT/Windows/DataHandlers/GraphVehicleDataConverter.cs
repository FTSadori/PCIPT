using OxyPlot;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Windows.ObjectCreators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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

        public static List<GraphVehiclesData> InitConvert(List<VehicleByRoutesRow> vehicleByRoutes, Dictionary<int, NegSize> nodesCoords, List<CargoTurnoverPointDto> points, out Dictionary<string, int> starts, out List<NegSize> biases)
        {
            List<GraphVehiclesData> result = new();
            starts = new();
            biases = new();

            Dictionary<string, List<NegSize>> typesAndStartPoints = new();
            List<string> machineAlreadyWas = new();
            foreach (var vehicleByRoute in vehicleByRoutes)
            {
                string key = $"{vehicleByRoute.Name}[{vehicleByRoute.Number}]";
                if (machineAlreadyWas.Contains(key))
                {
                    continue;
                }
                machineAlreadyWas.Add(key);
                if (!typesAndStartPoints.ContainsKey(vehicleByRoute.Name))
                {
                    typesAndStartPoints.Add(vehicleByRoute.Name, new());
                }

                if (vehicleByRoute.CargoCode != -1)
                {
                    var po = points.Where(p => p.Id == vehicleByRoute.PointId).First();
                    starts.Add(key, po.SourceId);
                }
                else
                {
                    starts.Add(key, vehicleByRoute.PointId);
                }

                double randomAngle = random.NextDouble() * 360.0;
                double randomLength = random.NextDouble() * RandomMaxRange;
                biases.Add(new(randomLength * Math.Cos(randomAngle), randomLength * Math.Sin(randomAngle)));
                NegSize randomizedCoords = new(
                    nodesCoords[starts.Last().Value].Width + randomLength * Math.Cos(randomAngle),
                    nodesCoords[starts.Last().Value].Height + randomLength * Math.Sin(randomAngle));
                typesAndStartPoints[vehicleByRoute.Name].Add(randomizedCoords);
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
