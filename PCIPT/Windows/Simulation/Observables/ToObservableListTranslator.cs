using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ToObservableListTranslator
    {
        public static ObservableCollection<ObservableVehicleObject> ObservableVehicles = new();

        public static void UpdateList(List<VehicleObject> vehicleObjects)
        {
            if (ObservableVehicles.Count == 0)
            {
                int id = 0;
                foreach (var vehicleObject in vehicleObjects)
                {
                    ObservableVehicleObject observableVehicleObject = new();
                    observableVehicleObject.Number = vehicleObject.number;
                    observableVehicleObject.Name = vehicleObject.name;
                    observableVehicleObject.Id = id++;
                    ObservableVehicles.Add(observableVehicleObject);
                }
            }

            
            for (int i = 0; i < vehicleObjects.Count; ++i)
            {
                ObservableVehicles[i].VehicleState = vehicleObjects[i].vehicleState;
                ObservableVehicles[i].LastNodeId = vehicleObjects[i].lastNodeId;
                ObservableVehicles[i].CurrentLoad = (float)vehicleObjects[i].load;
                ObservableVehicles[i].PointId = vehicleObjects[i].lastPointId;
            }
        }
    }
}
