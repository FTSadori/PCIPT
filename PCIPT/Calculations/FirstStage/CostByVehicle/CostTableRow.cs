using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.CostByVehicle
{
    public sealed record CostTableRow(string Name, float[] Costs, string FuelType);
}
