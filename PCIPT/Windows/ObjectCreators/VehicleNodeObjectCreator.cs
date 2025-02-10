using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace PCIPT.Windows.ObjectCreators
{
    public sealed class VehicleNodeObjectCreator
    {
        public const double EllipseSize = 10;

        public static Ellipse GetObject(Brush brush, double Size, NegSize Coord)
        {
            Ellipse obj = new()
            {
                Width = EllipseSize * Size,
                Height = EllipseSize * Size,
                Fill = brush,
            };

            Canvas.SetLeft(obj, Coord.Width);
            Canvas.SetBottom(obj, Coord.Height);

            return obj;
        }
    }
}
