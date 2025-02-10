using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Graph
{
    public sealed record NodesCoordsDto(int Id, float CoordX, float CoordY) : IData;
}
