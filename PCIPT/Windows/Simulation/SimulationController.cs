using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Graph;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PCIPT.Windows.Simulation.Observables;
using System.Windows.Controls;

namespace PCIPT.Windows.Simulation
{
    public sealed class SimulationController
    {
        static Random random = new();

        private List<VehicleByRoutesRow> plan;
        private List<ReroutingTask> pendingReroutingTasks = new();

        private List<VehicleTypeDto> vehicleTypes;

        public List<VehicleObject> vehicleObjects = new();
        public List<PointObject> pointObjects = new();

        private Dictionary<int, NegSize> nodesCoords;

        private string logFilePath;

        private List<int> cyclesLeft = new();
        private Performance performance = new();
        private TextBlock output;

        private double totalTime = 0;

        public SimulationController(List<VehicleByRoutesRow> plan, List<CargoTurnoverPointDto> pointsDto, List<VehicleTypeDto> vehicleTypes, List<CargoDto> cargoDtos, List<VehicleDto> vehicleDtos, List<GraphVehiclesData> vehiclesCoords, Dictionary<int, NegSize> nodesCoords, Dictionary<string, int> startPoints, List<NegSize> biases, string logFilePath, TextBlock output)
        {
            this.plan = plan;
            this.vehicleTypes = vehicleTypes;
            this.nodesCoords = nodesCoords;
            this.output = output;

            foreach (var dto in pointsDto)
            {
                var dest = nodesCoords[dto.DestinationId];
                var sour = nodesCoords[dto.SourceId];

                var cargo = cargoDtos.Where(c => c.Code == dto.CargoCode).First();

                pointObjects.Add(new PointObject(dto.Id, dto.OutgoingCargo, dto.OutgoingCargo, dto.OutgoingCargo, sour.Width, sour.Height, dest.Width, dest.Height, cargo.Type, cargo.CapacityUtilisationRate, RouteCalculator.GetDistanceBetween(dto.SourceId, dto.DestinationId), dto.SourceId, dto.DestinationId));
            }

            foreach (var line in plan)
            {
                cyclesLeft.Add((int)line.NumberOfCycles);
            }

            int i = 0;
            Dictionary<string, int> pairs = new();

            foreach (var dto in vehiclesCoords)
            {
                var vehicle = vehicleDtos.Where(v => v.Name == dto.Type).First();
                if (!pairs.ContainsKey(dto.Type))
                {
                    pairs.Add(dto.Type, 0);
                }
                pairs[dto.Type]++;

                //GraphWindow.This.Title += $" {i}:{vehicle.Type}:{startPoints[i]} ";
                string key = $"{vehicle.Name}[{pairs[dto.Type]}]";
                vehicleObjects.Add(new VehicleObject(pairs[dto.Type], dto.Type, vehicle.LoadCapacity, vehicle.LoadTime, vehicle.SpeedWithLoad, vehicle.SpeedWithoutLoad, vehicle.Type, startPoints[key], startPoints[key], biases[i]));
                ++i;
            }

            this.logFilePath = logFilePath;
        }

        public void AddNewReroutingTask(string machineName, int machineNumber, int pointId)
        {
            pendingReroutingTasks.Add(new ReroutingTask(machineName, machineNumber, pointId));
        }

        public void NextStep(double deltaSeconds, double effectiveTime)
        {
            totalTime += deltaSeconds / 60;

            StreamWriter sw = new(logFilePath, true);
            int id = 0;
            foreach (var vehicle in vehicleObjects)
            {
                StepForVehicle(id, sw, vehicle, deltaSeconds * (effectiveTime + random.NextDouble() * (1.0 - effectiveTime)), deltaSeconds);
                id += 1;
            }
            sw.Close();

            ToObservableListTranslator.UpdateList(vehicleObjects);
            ToObservablePointsListTranslator.UpdateList(pointObjects);
        }

        public void StepForVehicle(int id, StreamWriter sw, VehicleObject vehicle, double time, double deltaSeconds)
        {
            if (time <= 0) return;
            double timeN = 0;

            switch (vehicle.vehicleState)
            {
                case VehicleState.AWAITS:
                    timeN = Awaits(vehicle, sw, time, id);
                    ObservableListForVehicleAssessObject.AddTime(id, 0, (float)totalTime);
                    break;
                case VehicleState.MOVES_WITH_LOAD:
                    timeN = MovesWithLoad(vehicle, sw, time);
                    ObservableListForVehicleAssessObject.AddTime(id, (float)time - (float)timeN, (float)totalTime);
                    break;
                case VehicleState.MOVES_WITHOUT_LOAD:
                    timeN = MovesWithoutLoad(vehicle, sw, time);
                    ObservableListForVehicleAssessObject.AddTime(id, (float)time - (float)timeN, (float)totalTime);
                    break;
                case VehicleState.MOVES_BACK:
                    timeN = MovesBack(vehicle, sw, time);
                    ObservableListForVehicleAssessObject.AddTime(id, (float)time - (float)timeN, (float)totalTime);
                    break;
                case VehicleState.LOADS:
                    timeN = Loads(vehicle, sw, time);
                    ObservableListForVehicleAssessObject.AddTime(id, (float)time - (float)timeN, (float)totalTime);
                    break;
                case VehicleState.UNLOADS:
                    timeN = Unloads(vehicle, sw, time, id);
                    ObservableListForVehicleAssessObject.AddTime(id, (float)time - (float)timeN, (float)totalTime);
                    break;
                case VehicleState.OVER:
                    timeN = Over(vehicle, sw, time);
                    ObservableListForVehicleAssessObject.AddTime(id, 0, (float)totalTime);
                    break;
                    
            }
            StepForVehicle(id, sw, vehicle, timeN, deltaSeconds);
        }

        public double Over(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            var reroutingTask = pendingReroutingTasks.Find(r => r.machine == vehicle.name && r.number == vehicle.number);
            if (reroutingTask != null)
            {
                ChangeState(vehicle, sw, VehicleState.AWAITS);
                return deltaSeconds;
            }
            return 0;
        }

        public void ChangeState(VehicleObject vehicle, StreamWriter sw, VehicleState state)
        {
            AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) changed state from {vehicle.vehicleState} to {state}");
            vehicle.vehicleState = state;
        }

        public double MovesBack(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            int sourceId = vehicle.path[vehicle.pathIterator];
            int destinationId = vehicle.path[vehicle.pathIterator + 1];
            double dist = RouteCalculator.GetDistanceBetween(sourceId, destinationId);

            double delta = vehicle.speedWithoutCargo * deltaSeconds / dist;
            vehicle.lPassed += delta;

            if (vehicle.lPassed >= 1)
            {
                double secondsSaved = (vehicle.lPassed - 1) / delta * deltaSeconds;

                vehicle.lPassed = 0;
                vehicle.lastNodeId = destinationId;
                vehicle.pathIterator++;

                if (vehicle.pathIterator >= vehicle.path.Count - 1)
                {
                    vehicle.loadTimeRemaining = vehicle.loadTime;
                    ChangeState(vehicle, sw, VehicleState.OVER);
                    return secondsSaved;
                }
                AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
                return secondsSaved;
            }
            return 0;
        }

        public double MovesWithoutLoad(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            int sourceId = vehicle.path[vehicle.pathIterator];
            int destinationId = vehicle.path[vehicle.pathIterator + 1];
            double dist = RouteCalculator.GetDistanceBetween(sourceId, destinationId);

            double delta = vehicle.speedWithoutCargo * deltaSeconds / dist;
            vehicle.lPassed += delta;
            //AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) moved without load by {delta * 100:0.00}% (total of {Math.Min(vehicle.lPassed * 100, 100):0.00}%)");

            if (vehicle.lPassed >= 1)
            {
                double secondsSaved = (vehicle.lPassed - 1) / delta * deltaSeconds;

                vehicle.lPassed = 0;
                vehicle.lastNodeId = destinationId;
                vehicle.pathIterator++;

                if (vehicle.pathIterator >= vehicle.path.Count - 1)
                {
                    vehicle.loadTimeRemaining = vehicle.loadTime;
                    ChangeState(vehicle, sw, VehicleState.LOADS);
                    return secondsSaved;
                }
                AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
                return secondsSaved;
            }
            return 0;
        }

        public double Unloads(VehicleObject vehicle, StreamWriter sw, double deltaSeconds, int id)
        {
            if (vehicle.loadTimeRemaining < 0)
            {
                var reroutingTask = pendingReroutingTasks.Find(r => r.machine == vehicle.name && r.number == vehicle.number);
                if (reroutingTask != null)
                {
                    ChangeState(vehicle, sw, VehicleState.AWAITS);
                    return deltaSeconds;
                }

                var po = pointObjects.Where(po => po.pointId == vehicle.lastPointId).First();

                ObservableListForVehicleAssessObject.AddCargo(id, (float)vehicle.load);
                if (vehicle.planLineId != -1)
                {
                    cyclesLeft[vehicle.planLineId] -= 1;
                }
                if (po.cargoLeft > 0 && (vehicle.planLineId == -1 || cyclesLeft[vehicle.planLineId] != 0))
                {
                    // get ahead of time
                    po.cargoLeft = Math.Max(-0.01, po.cargoLeft - vehicle.maxLoad * po.utilizationRate);
                    //

                    var path = RouteCalculator.GetPathBetween(vehicle.lastNodeId, po.fromId);
                    if (path == null)
                    {
                        AddToLog(sw,$"=========================== ERROR ===========================");
                        AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {po.fromId}");
                        AddToLog(sw,$"=============================================================");
                        return 0;
                    }

                    vehicle.path = path.Nodes;
                    vehicle.pathIterator = 0;
                    vehicle.lPassed = 0;
                    ChangeState(vehicle, sw, VehicleState.MOVES_WITHOUT_LOAD);
                    vehicle.load = 0;
                    return deltaSeconds;
                }

                ObservableListForVehicleAssessObject.DoneRequest(id, 1);
                ChangeState(vehicle, sw, VehicleState.AWAITS);
                return deltaSeconds;
            }
            else
            {
                vehicle.loadTimeRemaining -= deltaSeconds;
                if (vehicle.loadTimeRemaining < 0)
                {
                    return -vehicle.loadTimeRemaining;
                }
                return 0;
            }
        }

        public double Loads(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            if (vehicle.loadTimeRemaining < 0)
            {
                var po = pointObjects.Where(po => po.pointId == vehicle.lastPointId).First();

                performance.loadTimes += 1;

                var path = RouteCalculator.GetPathBetween(vehicle.lastNodeId, po.toId);
                if (path == null)
                {
                    AddToLog(sw,$"=========================== ERROR ===========================");
                    AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {po.toId}");
                    AddToLog(sw,$"=============================================================");
                    return 0;
                }
                double newload = Math.Min(po.actualCargoLeft, vehicle.maxLoad * po.utilizationRate);

                po.actualCargoLeft = Math.Max(0, po.actualCargoLeft - vehicle.maxLoad * po.utilizationRate);

                vehicle.path = path.Nodes;
                vehicle.pathIterator = 0;
                vehicle.lPassed = 0;
                ChangeState(vehicle, sw, VehicleState.MOVES_WITH_LOAD);
                vehicle.load = newload;
                return deltaSeconds;
            }
            else
            {
                vehicle.loadTimeRemaining -= deltaSeconds;
                if (vehicle.loadTimeRemaining < 0)
                {
                    return -vehicle.loadTimeRemaining;
                }
                return 0;
            }
        }

        public double MovesWithLoad(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            int sourceId = vehicle.path[vehicle.pathIterator];
            int destinationId = vehicle.path[vehicle.pathIterator + 1];
            double dist = RouteCalculator.GetDistanceBetween(sourceId, destinationId);

            double delta = vehicle.speedWithCargo * deltaSeconds / dist;
            vehicle.lPassed += delta;
            //AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) moved with load by {delta*100:0.00}% (total of {Math.Min(vehicle.lPassed*100, 100):0.00}%)");

            if (vehicle.lPassed >= 1)
            {
                double secondsSaved = (vehicle.lPassed - 1) / delta * deltaSeconds;

                vehicle.lPassed = 0;
                vehicle.lastNodeId = destinationId;
                vehicle.pathIterator++;

                if (vehicle.pathIterator >= vehicle.path.Count - 1)
                {
                    var po = pointObjects.Where(po => po.pointId == vehicle.lastPointId).First();
                    vehicle.loadTimeRemaining = vehicle.loadTime;
                    ChangeState(vehicle, sw, VehicleState.UNLOADS);
                    return secondsSaved;
                }

                AddToLog(sw,$"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
                return secondsSaved;
            }
            return 0;
        }

        public double Awaits(VehicleObject vehicle, StreamWriter sw, double deltaSeconds, int id)
        {
            var pointsForVehicle = plan.Where(p => p.Name == vehicle.name && p.Number == vehicle.number).ToList();

            var reroutingTask = pendingReroutingTasks.Find(r => r.machine == vehicle.name && r.number == vehicle.number);
            if (reroutingTask != null)
            {
                pendingReroutingTasks.Remove(reroutingTask);
                if (reroutingTask.pointId == -1)
                {
                    ChangeState(vehicle, sw, VehicleState.OVER);
                    return 0;
                }
                pointsForVehicle.Insert(0, new VehicleByRoutesRow("", 0, reroutingTask.pointId, 0, 0, 0, 0, -1, 0));
                ObservableListForVehicleAssessObject.AddRequest(id, 1);
            }

            if (pointsForVehicle.Count == 0)
            {
                return 0;
            }

            foreach (var point in pointsForVehicle)
            {
                int pointId = plan.IndexOf(point);

                var po = pointObjects.Where(po => po.pointId == point.PointId).First();
                if (po.cargoLeft > 0 && (pointId == -1 || cyclesLeft[pointId] != 0))
                {
                    // get ahead of time
                    po.cargoLeft = Math.Max(-0.001, po.cargoLeft - vehicle.maxLoad * po.utilizationRate);
                    //

                    if (point.NumberOfCycles != -1)
                        vehicle.planLineId = plan.IndexOf(point);
                    else
                        vehicle.planLineId = -1;

                    if (vehicle.lastNodeId == po.fromId)
                    {
                        vehicle.loadTimeRemaining = vehicle.loadTime;
                        vehicle.lPassed = 0;
                        vehicle.lastPointId = point.PointId;
                        ChangeState(vehicle, sw, VehicleState.LOADS);
                        return deltaSeconds;
                    }

                    var path = GetPath(vehicle.lastNodeId, po.fromId, sw, vehicle);
                    if (path == null) return 0;
                    if (path.Nodes.Count == 0)
                    {
                        ChangeState(vehicle, sw, VehicleState.LOADS);
                        return deltaSeconds;
                    }

                    vehicle.path = path.Nodes;
                    vehicle.pathIterator = 0;
                    vehicle.lPassed = 0;
                    vehicle.lastPointId = point.PointId;
                    ChangeState(vehicle, sw, VehicleState.MOVES_WITHOUT_LOAD);
                    return deltaSeconds;
                }
            }

            var path2 = GetPath(vehicle.lastNodeId, vehicle.startId, sw, vehicle);
            if (path2 == null) return 0;
            if (path2.Nodes.Count == 0)
            {
                ChangeState(vehicle, sw, VehicleState.OVER);
                return deltaSeconds;
            }
            
            vehicle.path = path2.Nodes;
            vehicle.pathIterator = 0;
            vehicle.lPassed = 0;
            ChangeState(vehicle, sw, VehicleState.MOVES_BACK);

            return deltaSeconds;
        }

        public RouteCalculator.Path? GetPath(int from, int to, StreamWriter sw, VehicleObject vehicle)
        {
            if (from == to)
            {
                return new RouteCalculator.Path(new List<int>(), 0);
            }
            var path = RouteCalculator.GetPathBetween(from, to);
            if (path == null)
            {
                AddToLog(sw, $"=========================== ERROR ===========================");
                AddToLog(sw, $"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {from} to node {to}");
                AddToLog(sw, $"=============================================================");
                return null;
            }
            
            string str = "";
            foreach(var p in path.Nodes)
            {
                str += p + ">";
            }
            AddToLog(sw, $"{vehicle.name}:{vehicle.number} from {from} to {to} but " + str[0..^1] + ":" + path.TotalDistance);
            
            return path;
        }

        public void SaveStats()
        {
            StreamWriter sw = new("PERFORMANCE.TXT");
            AddToLog(sw,$"Average load time: {performance.AverageLoadTime}");
            AddToLog(sw,$"Average unload time: {performance.AverageUnloadTime}");
            sw.Close();
        }

        public List<GraphVehiclesData> GetNewGraphVehiclesData()
        {
            List<GraphVehiclesData> graphVehiclesDatas = new();

            foreach (var vehicle in vehicleObjects)
            {
                if (vehicle.vehicleState == VehicleState.MOVES_WITH_LOAD ||
                    vehicle.vehicleState == VehicleState.MOVES_WITHOUT_LOAD ||
                    vehicle.vehicleState == VehicleState.MOVES_BACK)
                {
                    var coordFirst = nodesCoords[vehicle.path[vehicle.pathIterator]];
                    var coordSecond = nodesCoords[vehicle.path[vehicle.pathIterator + 1]];

                    graphVehiclesDatas.Add(new GraphVehiclesData(vehicle.name,
                        new((coordFirst.Width) * (1.0 - vehicle.lPassed) + (coordSecond.Width) * vehicle.lPassed,
                        (coordFirst.Height) * (1.0 - vehicle.lPassed) + (coordSecond.Height) * vehicle.lPassed))
                        );
                }
                else
                {
                    var coord = nodesCoords[vehicle.lastNodeId];

                    graphVehiclesDatas.Add(new GraphVehiclesData(vehicle.name, new(coord.Width, coord.Height)));
                }
            }

            return graphVehiclesDatas;
        }


        public void AddToLog(StreamWriter sw, string text)
        {
            sw.WriteLine(text);
            GraphWindow.DoCmd(delegate ()
            {
                output.Text += text + "\n";
            });
        }
    }
}
