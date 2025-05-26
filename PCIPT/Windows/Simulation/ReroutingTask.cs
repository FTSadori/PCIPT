using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation
{
    public sealed record ReroutingTask(string machine, int number, int pointId);
}
