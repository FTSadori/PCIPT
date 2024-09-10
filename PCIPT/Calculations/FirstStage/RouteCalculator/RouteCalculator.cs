using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.RouteFinder
{
    public sealed class RouteCalculator
    {
        public static float GetDistanceBetween(int startPointId, int endPointId)
        {
            float dist = routeDtos.Find(r => r.SourceId == startPointId && r.DestinationId == endPointId)?.Distance ??
                         //routeDtos.Find(r => r.SourceId == endPointId && r.DestinationId == startPointId)?.Distance ??
                         float.PositiveInfinity;

            if (dist != float.PositiveInfinity) 
                return dist;
            
            string var = startPointId + "_" + endPointId;

            if (savedRoutes.ContainsKey(var))
                return savedRoutes[var].TotalDistance;

            Path? path = DijkstraAlgorithm(startPointId, endPointId);

            if (path == null) 
                return float.PositiveInfinity;
            savedRoutes.Add(var, path);
            return path.TotalDistance;
        }

        public static void InitRoutes(List<RouteDto> routes, List<NodeDto> nodes)
        {
            routeDtos = routes.ToList();
            nodeDtos = nodes.ToList();
            nodeCount = nodeDtos.Count;
        }

        private static Path? DijkstraAlgorithm(int startPointId, int endPointId)
        {
            Dictionary<int, float> vertexSum = new();
            foreach (var node in nodeDtos) vertexSum.Add(node.Id, float.PositiveInfinity);
            vertexSum[startPointId] = 0;

            Dictionary<int, bool> vertexClosed = new();
            foreach (var node in nodeDtos) vertexClosed.Add(node.Id, false);

            Dictionary<int, int> vertexFrom = new();
            foreach (var node in nodeDtos) vertexFrom.Add(node.Id, -1);

            List<int> controlVertexes = new() { startPointId };

            while (controlVertexes.Count > 0)
            {
                List<int> newControlVertexes = new();

                foreach (var fromVertex in controlVertexes)
                {
                    foreach (var route in routeDtos.Where(r => r.SourceId == fromVertex && !vertexClosed[r.DestinationId]))
                    {
                        if (vertexSum[route.DestinationId] > vertexSum[fromVertex] + route.Distance)
                        {
                            vertexFrom[route.DestinationId] = fromVertex;
                            vertexSum[route.DestinationId] = vertexSum[fromVertex] + route.Distance;
                            newControlVertexes.Add(route.DestinationId);
                        }
                    }
                    vertexClosed[fromVertex] = true;
                }

                if (newControlVertexes.Count == 1 && newControlVertexes[0] == endPointId)
                    break;

                controlVertexes = newControlVertexes.ToList();
            }

            if (vertexFrom[endPointId] == -1)
                return null;

            List<int> Nodes = new() { endPointId };
            int vertex = endPointId;
            while (vertex != startPointId)
            {
                vertex = vertexFrom[vertex];
                Nodes.Add(vertex);
            }

            return new Path(Nodes, vertexSum[endPointId]);
        }

        private sealed record Path(List<int> Nodes, float TotalDistance);

        private static List<NodeDto> nodeDtos = new();
        private static List<RouteDto> routeDtos = new();
        private static Dictionary<string, Path> savedRoutes = new();
        private static int nodeCount = 0;
    }
}
