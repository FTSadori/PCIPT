using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.Routes
{
    public sealed record RouteDto(int Id, string SourceName, string DestinationName, float Distance) : IData;
}
