using PCIPT.Core;
using PCIPT.Windows.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCIPT.Windows.ObjectCreators
{
    public sealed class AllVehicleNodesObjectCreator
    {
        private static Dictionary<string, Color> pickedColors = new();
        private const int colorDistance = 30;
        private const int maxLightness = 512;

        public static void ClearAllColors()
        {
            pickedColors = new();
        }

        public static List<Ellipse> GetObjects(List<GraphVehiclesData> vehiclesDatas, double size)
        {
            List<Ellipse> ellipses = new();

            foreach (var data in vehiclesDatas)
            {
                if (!pickedColors.ContainsKey(data.Type))
                {
                    pickedColors.Add(data.Type, MakeDifferentColor());
                }
                NegSize neg = new(
                    data.Coord.Width - VehicleNodeObjectCreator.EllipseSize / 2 * size,
                    data.Coord.Height - VehicleNodeObjectCreator.EllipseSize / 2 * size);
                ellipses.Add(VehicleNodeObjectCreator.GetObject(new SolidColorBrush(pickedColors[data.Type]), size, neg));
            }

            return ellipses;
        }

        private static Color MakeDifferentColor()
        {
            Color c;
            bool bad;

            do
            {
                c = RandomColorPicker.GetRandomColor();
                bad = false;

                int lightness = c.R + c.G + c.B;
                if (lightness >= maxLightness)
                {
                    bad = true;
                    continue;
                }

                foreach (var pair in pickedColors)
                {
                    int d = 0;

                    d += Math.Abs(pair.Value.R - c.R);
                    d += Math.Abs(pair.Value.G - c.G);
                    d += Math.Abs(pair.Value.B - c.B);

                    if (d <= colorDistance)
                    {
                        bad = true;
                        break;
                    }
                }
            } while (bad);

            return c;
        }
    }
}
