using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Cargoes
{
    public sealed record CargoDto(int Code, string Name, string Type, float CapacityUtilisationRate) : IData;
}
