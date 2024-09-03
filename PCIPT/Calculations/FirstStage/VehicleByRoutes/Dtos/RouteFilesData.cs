using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos
{
    public record RouteFilesData(int PointId, string FileName, float Distance)
    {
        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
    }
}
