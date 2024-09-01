using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.CostWeight
{
    public sealed record CostWeightDto(string Name, float Weight) : IData;
}
