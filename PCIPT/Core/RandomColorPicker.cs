using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Xps.Serialization;

namespace PCIPT.Core
{
    public sealed class RandomColorPicker
    {
        static Random random = new();

        public static Color GetRandomColor()
        {
            Color color = new()
            {
                R = (byte)random.Next(0, 255),
                G = (byte)random.Next(0, 255),
                B = (byte)random.Next(0, 255),
                A = 255
            };
            return color;
        }
    }
}
