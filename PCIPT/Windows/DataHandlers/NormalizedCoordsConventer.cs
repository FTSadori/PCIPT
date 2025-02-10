using PCIPT.Dtos.Graph;
using PCIPT.Dtos.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PCIPT.Windows.DataHandlers
{
    public sealed class NormalizedCoordsConventer
    {
        public static Dictionary<int, NegSize> ConvertToCircle(List<NodeDto> nodes)
        {
            Dictionary<int, NegSize> nodesCoords = new();

            int N = nodes.Count;

            double angle = 2.0 * Math.PI / N;

            for (int i = 0; i < N; i++)
            {
                nodesCoords.Add(nodes[i].Id, new NegSize(Math.Cos(i * angle), Math.Sin(i * angle)));
            }

            return nodesCoords;
        }

        public static Dictionary<int, NegSize> ConvertFromOtherCoords(List<NodesCoordsDto> nodesCoordsDtos)
        {
            if (nodesCoordsDtos.Count == 0) return new();

            Dictionary<int, NegSize> nodesCoords = new();

            double minX = nodesCoordsDtos[0].CoordX;
            double minY = nodesCoordsDtos[0].CoordY;
            double maxX = minX;
            double maxY = minY;

            foreach (var node in nodesCoordsDtos)
            {
                minX = Math.Min(minX, node.CoordX);
                maxX = Math.Max(maxX, node.CoordX);
                minY = Math.Min(minY, node.CoordY);
                maxY = Math.Max(maxY, node.CoordY);
            }

            double DeltaX = maxX - minX;
            double DeltaY = maxY - minY;

            double MidX = (maxX + minX) / 2;
            double MidY = (maxY + minY) / 2;

            foreach (var node in nodesCoordsDtos)
            {
                nodesCoords.Add(node.Id, new((node.CoordX - MidX) / DeltaX * 2, (node.CoordY - MidY) / DeltaY * 2));
            }

            return nodesCoords;
        }
    }
}
