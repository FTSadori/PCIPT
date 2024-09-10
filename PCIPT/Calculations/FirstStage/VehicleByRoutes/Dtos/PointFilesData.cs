using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos
{
    public record PointFilesData(int Id, string FileName, float Distance, int SourceId, int DestinationId)
    {
        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
    }
}
