using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.CargoStats.Dtos
{
    public sealed record CargoMass(int CargoCode, float TotalMass);
}
