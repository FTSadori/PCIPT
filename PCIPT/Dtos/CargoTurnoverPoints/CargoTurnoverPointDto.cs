using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.CargoTurnoverPoints
{
    public sealed record CargoTurnoverPointDto(int Id, int SourceId, int DestinationId, float OutgoingCargo, int CargoCode) : IData;
}
