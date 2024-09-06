using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Node
{
    public sealed record NodeDto(int Id, string Name) : IData;
}
