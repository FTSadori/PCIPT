using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableListForCargoAssessObjects
    {
        public static ObservableCollection<ObservableCargoAssessObject> ObservableCargoes = new();

        public static Dictionary<int, float> maxCargoes = new();

        public static void UpdateList(List<PointObject> pointObjects)
        {
            if (ObservableCargoes.Count == 0)
            {
                maxCargoes.Clear();
                foreach (var cargoDto in ToObservablePointsListTranslator.cargoDtos)
                {
                    var fullCargo = ToObservablePointsListTranslator.cargoTurnoverPointDtos.Where(p => p.CargoCode == cargoDto.Code).Sum(p => p.OutgoingCargo);

                    ObservableCargoAssessObject cargoObject = new();
                    cargoObject.Code = cargoDto.Code;
                    cargoObject.Name = cargoDto.Name;
                    ObservableCargoes.Add(cargoObject);

                    maxCargoes.Add(cargoDto.Code, fullCargo);
                }
            }

            foreach (var cargoDto in ToObservablePointsListTranslator.cargoDtos)
            {
                var points = ToObservablePointsListTranslator.cargoTurnoverPointDtos.Where(p => p.CargoCode == cargoDto.Code);
                float transported = 0;
                foreach (var point in points)
                {
                    var specPoint = pointObjects.Find(p => p.pointId == point.Id);
                    transported += (float)(specPoint.allDailyCargo - specPoint.actualCargoLeft);
                }
                var ocargo = ObservableCargoes.ToList().Find(o => o.Code == cargoDto.Code);
                ocargo.CargoTransported = transported;
                ocargo.PersentageDone = MathF.Round(transported / maxCargoes[cargoDto.Code] * 100, 2);
            }
        }
    }
}
