using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos
{
    public sealed record VehicleByRoutesRow(string Name, int Number, int PointId, float FractionOfTimeUsed, float Distance, int CargoCode, float CycleDuration, float NumberOfCycles, float TotalTime);
}
