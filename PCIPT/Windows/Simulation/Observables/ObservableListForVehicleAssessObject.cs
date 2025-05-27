using OxyPlot.Series;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableListForVehicleAssessObject
    {
        public static ObservableCollection<ObservableVehicleAssessObject> ObservableVehicleAssesses = new();

        static List<float> allCargoPlan = new();

        public static void Init(List<VehicleObject> vehicleObjects, List<VehicleByRoutesRow> plan, List<PointObject> pointObjects)
        {
            ObservableVehicleAssesses.Clear();
            allCargoPlan.Clear();

            int id = 0;
            foreach (var vehicleObject in vehicleObjects)
            {
                ObservableVehicleAssessObject observableVehicleObject = new();
                observableVehicleObject.Number = vehicleObject.number;
                observableVehicleObject.Name = vehicleObject.name;
                observableVehicleObject.RequestsTotal = plan.Where(m => m.Number == vehicleObject.number && m.Name == vehicleObject.name).Count();
                observableVehicleObject.Id = id++;
                ObservableVehicleAssesses.Add(observableVehicleObject);
                allCargoPlan.Add(0);

                foreach (var line in plan.Where(p => p.Name == vehicleObject.name && p.Number == vehicleObject.number))
                {
                    float ur = (float)pointObjects.Find(point => point.pointId == line.PointId).utilizationRate;
                    float c = (float)vehicleObjects.Find(vo => vo.name == vehicleObject.name && vo.number == vehicleObject.number).maxLoad;
                    allCargoPlan[^1] += (line.NumberOfCycles - 1) * ur * c + 0.0001f;
                }
            }
        }

        public static void AddTime(int id, float deltaTime, float totalTimePassed)
        {
            ObservableVehicleAssesses[id].OperatingTime += deltaTime / 60;
            ObservableVehicleAssesses[id].OperatingTimePersentage = (totalTimePassed == 0) ? 100 : MathF.Round((ObservableVehicleAssesses[id].OperatingTime / totalTimePassed) * 100, 2);
        }

        public static void AddRequest(int id, int requestNumber)
        {
            ObservableVehicleAssesses[id].RequestsTotal += requestNumber;
            ObservableVehicleAssesses[id].RequestsDonePersentage = (ObservableVehicleAssesses[id].RequestsTotal == 0) ? 100 : MathF.Round(ObservableVehicleAssesses[id].RequestsDone / ObservableVehicleAssesses[id].RequestsTotal * 100, 2);
        }

        public static void DoneRequest(int id, int requestNumber)
        {
            ObservableVehicleAssesses[id].RequestsDone += requestNumber;
            ObservableVehicleAssesses[id].RequestsDonePersentage = (ObservableVehicleAssesses[id].RequestsTotal == 0) ? 100 : MathF.Round(ObservableVehicleAssesses[id].RequestsDone / ObservableVehicleAssesses[id].RequestsTotal * 100, 2);
        }

        public static void AddCargo(int id, float cargo)
        {
            ObservableVehicleAssesses[id].TotalCargoTransported += cargo;
            ObservableVehicleAssesses[id].WorkDonePersentage = (allCargoPlan[id] == 0) ? 100 : MathF.Min(100f, MathF.Round(ObservableVehicleAssesses[id].TotalCargoTransported / allCargoPlan[id] * 100, 2));
        }
    }
}
