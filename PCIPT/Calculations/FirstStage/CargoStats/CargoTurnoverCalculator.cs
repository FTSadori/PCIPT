using PCIPT.Dtos.CargoTurnoverPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.CargoStats
{
    public sealed class CargoTurnoverCalculator
    {
        public static Dictionary<int, float> GetTotalMassForEachCargo(List<CargoTurnoverPointDto> points)
        {
            var totalMasses = new Dictionary<int, float>();

            foreach (var point in points)
            {
                if (totalMasses.ContainsKey(point.CargoCode))
                    totalMasses[point.CargoCode] += point.OutgoingCargo;
                else
                    totalMasses[point.CargoCode] = point.OutgoingCargo;
            }

            return totalMasses;
        }
    }
}
