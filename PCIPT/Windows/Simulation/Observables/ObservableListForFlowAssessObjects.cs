using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Node;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableListForFlowAssessObjects
    {
        public static ObservableCollection<ObservableFlowAssessObject> ObservableFlows = new();

        public static void UpdateList(List<PointObject> pointObjects)
        {
            if (ObservableFlows.Count == 0)
            {
                foreach (var pointObject in pointObjects)
                {
                    var po = ToObservablePointsListTranslator.cargoTurnoverPointDtos.Find(p => p.Id == pointObject.pointId);
                    var cg = ToObservablePointsListTranslator.cargoDtos.Find(c => c.Code == po.CargoCode);
                    var nd1 = ToObservablePointsListTranslator.nodeDtos.Find(n => n.Id == po.SourceId);
                    var nd2 = ToObservablePointsListTranslator.nodeDtos.Find(n => n.Id == po.DestinationId);

                    ObservableFlowAssessObject observablePointObject = new();
                    observablePointObject.Id = pointObject.pointId;
                    observablePointObject.Source = nd1.Name;
                    observablePointObject.Destination = nd2.Name;
                    observablePointObject.CargoName = cg.Name;
                    ObservableFlows.Add(observablePointObject);
                }
            }

            for (int i = 0; i < pointObjects.Count; ++i)
            {
                ObservableFlows[i].CargoTransported = (float)pointObjects[i].allDailyCargo - (float)pointObjects[i].actualCargoLeft;
                ObservableFlows[i].PersentageDone = MathF.Round(ObservableFlows[i].CargoTransported / (float)pointObjects[i].allDailyCargo * 100, 2);
            }
        }
    }
}
