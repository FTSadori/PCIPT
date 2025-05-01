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
    public sealed class ToObservablePointsListTranslator
    {
        public static ObservableCollection<ObservablePointObject> ObservablePoints = new();

        public static List<CargoTurnoverPointDto> cargoTurnoverPointDtos;
        public static List<CargoDto> cargoDtos;
        public static List<NodeDto> nodeDtos;

        public static void UpdateList(List<PointObject> pointObjects)
        {
            if (ObservablePoints.Count == 0)
            {
                foreach (var pointObject in pointObjects)
                {
                    var po = cargoTurnoverPointDtos.Find(p => p.Id == pointObject.pointId);
                    var cg = cargoDtos.Find(c => c.Code == po.CargoCode);
                    var nd1 = nodeDtos.Find(n => n.Id == po.SourceId);
                    var nd2 = nodeDtos.Find(n => n.Id == po.DestinationId);

                    ObservablePointObject observablePointObject = new();
                    observablePointObject.Id = pointObject.pointId;
                    observablePointObject.Source = nd1.Name;
                    observablePointObject.Destination = nd2.Name;
                    observablePointObject.CargoName = cg.Name;
                    ObservablePoints.Add(observablePointObject);
                }
            }


            for (int i = 0; i < pointObjects.Count; ++i)
            {
                ObservablePoints[i].CargoLeft = (float)pointObjects[i].actualCargoLeft;
                ObservablePoints[i].FractionLeft = (float)pointObjects[i].actualCargoLeft / (float)pointObjects[i].allDailyCargo;
            }
        }
    }
}
