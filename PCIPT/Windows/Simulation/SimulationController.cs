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

namespace PCIPT.Windows.Simulation
{
    public sealed class SimulationController
    {
        static Random random = new();

        private List<VehicleByRoutesRow> plan;

        private List<VehicleTypeDto> vehicleTypes;

        private List<VehicleObject> vehicleObjects = new();
        private List<PointObject> pointObjects = new();

        private Dictionary<int, NegSize> nodesCoords;

        private string logFilePath;

        private Performance performance = new();

        public SimulationController(List<VehicleByRoutesRow> plan, List<CargoTurnoverPointDto> pointsDto, List<VehicleTypeDto> vehicleTypes, List<CargoDto> cargoDtos, List<VehicleDto> vehicleDtos, List<GraphVehiclesData> vehiclesCoords, Dictionary<int, NegSize> nodesCoords, List<int> startPoints, List<NegSize> biases, string logFilePath)
        {
            this.plan = plan;
            this.vehicleTypes = vehicleTypes;
            this.nodesCoords = nodesCoords;

            foreach (var dto in pointsDto)
            {
                var dest = nodesCoords[dto.DestinationId];
                var sour = nodesCoords[dto.SourceId];

                var cargo = cargoDtos.Where(c => c.Code == dto.CargoCode).First();

                pointObjects.Add(new PointObject(dto.Id, dto.OutgoingCargo, sour.Width, sour.Height, dest.Width, dest.Height, cargo.Type, cargo.CapacityUtilisationRate, RouteCalculator.GetDistanceBetween(dto.SourceId, dto.DestinationId), dto.SourceId, dto.DestinationId));
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

                vehicleObjects.Add(new VehicleObject(pairs[dto.Type], dto.Type, vehicle.LoadCapacity, vehicle.LoadTime, vehicle.SpeedWithLoad, vehicle.SpeedWithoutLoad, vehicle.Type, startPoints[i], startPoints[i], biases[i]));
                ++i;
            }

            this.logFilePath = logFilePath;
        }

        public void NextStep(double deltaSeconds, double effectiveTime)
        {
            StreamWriter sw = new(logFilePath, true);
            foreach (var vehicle in vehicleObjects)
            {
                StepForVehicle(sw, vehicle, deltaSeconds * (effectiveTime + random.NextDouble() * (1.0 - effectiveTime)), deltaSeconds);
            }
            sw.Close();
        }

        public void StepForVehicle(StreamWriter sw, VehicleObject vehicle, double time, double deltaSeconds)
        {
            if (time <= 0) return;

            switch (vehicle.vehicleState)
            {
                case VehicleState.AWAITS:
                    Awaits(vehicle, sw);
                    break;
                case VehicleState.MOVES_WITH_LOAD:
                    time = MovesWithLoad(vehicle, sw, time);
                    break;
                case VehicleState.MOVES_WITHOUT_LOAD:
                    time = MovesWithoutLoad(vehicle, sw, time);
                    break;
                case VehicleState.MOVES_BACK:
                    time = MovesBack(vehicle, sw, time);
                    break;
                case VehicleState.LOADS:
                    performance.accumulatedLoadTime += deltaSeconds;
                    time = Loads(vehicle, sw, time);
                    break;
                case VehicleState.UNLOADS:
                    performance.accumulatedUnloadTime += deltaSeconds;
                    time = Unloads(vehicle, sw, time);
                    break;
                case VehicleState.OVER:
                    return;
            }
            StepForVehicle(sw, vehicle, time, deltaSeconds);
        }

        public void ChangeState(VehicleObject vehicle, StreamWriter sw, VehicleState state)
        {
            sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) changed state from {vehicle.vehicleState} to {state}");
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
                sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
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
            //sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) moved without load by {delta * 100:0.00}% (total of {Math.Min(vehicle.lPassed * 100, 100):0.00}%)");

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
                sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
                return secondsSaved;
            }
            return 0;
        }

        public double Unloads(VehicleObject vehicle, StreamWriter sw, double deltaSeconds)
        {
            if (vehicle.loadTimeRemaining < 0)
            {
                var po = pointObjects.Where(po => po.pointId == vehicle.lastPointId).First();

                performance.unloadTimes += 1;
                if (po.cargoLeft > 0)
                {
                    // get ahead of time
                    po.cargoLeft = Math.Max(-0.01, po.cargoLeft - vehicle.maxLoad * po.utilizationRate);
                    //

                    var path = RouteCalculator.GetPathBetween(vehicle.lastNodeId, po.fromId);
                    if (path == null)
                    {
                        sw.WriteLine($"=========================== ERROR ===========================");
                        sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {po.fromId}");
                        sw.WriteLine($"=============================================================");
                        return 0;
                    }

                    vehicle.path = path.Nodes;
                    vehicle.pathIterator = 0;
                    vehicle.lPassed = 0;
                    ChangeState(vehicle, sw, VehicleState.MOVES_WITHOUT_LOAD);
                    return deltaSeconds;
                }

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
                    sw.WriteLine($"=========================== ERROR ===========================");
                    sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {po.toId}");
                    sw.WriteLine($"=============================================================");
                    return 0;
                }

                vehicle.path = path.Nodes;
                vehicle.pathIterator = 0;
                vehicle.lPassed = 0;
                ChangeState(vehicle, sw, VehicleState.MOVES_WITH_LOAD);
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
            //sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId}) moved with load by {delta*100:0.00}% (total of {Math.Min(vehicle.lPassed*100, 100):0.00}%)");

            if (vehicle.lPassed >= 1)
            {
                double secondsSaved = (vehicle.lPassed - 1) / delta * deltaSeconds;

                vehicle.lPassed = 0;
                vehicle.lastNodeId = destinationId;
                vehicle.pathIterator++;

                if (vehicle.pathIterator >= vehicle.path.Count - 1)
                {
                    var po = pointObjects.Where(po => po.pointId == vehicle.lastPointId).First();
                    vehicle.load = vehicle.maxLoad * po.utilizationRate;
                    vehicle.loadTimeRemaining = vehicle.loadTime;
                    ChangeState(vehicle, sw, VehicleState.UNLOADS);
                    return secondsSaved;
                }

                sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} in ({vehicle.lastNodeId})");
                return secondsSaved;
            }
            return 0;
        }

        public void Awaits(VehicleObject vehicle, StreamWriter sw)
        {
            var pointsForVehicle = plan.Where(p => p.Name == vehicle.name && p.Number == vehicle.number).ToList();
            foreach (var point in pointsForVehicle)
            {
                var po = pointObjects.Where(po => po.pointId == point.PointId).First();
                if (po.cargoLeft > 0)
                {
                    // get ahead of time
                    po.cargoLeft = Math.Max(-0.01, po.cargoLeft - vehicle.maxLoad * po.utilizationRate);
                    //

                    if (vehicle.lastNodeId == po.fromId)
                    {
                        vehicle.loadTimeRemaining = vehicle.loadTime;
                        vehicle.lPassed = 0;
                        vehicle.lastPointId = point.PointId;
                        ChangeState(vehicle, sw, VehicleState.LOADS);
                        return;
                    }

                    var path = RouteCalculator.GetPathBetween(vehicle.lastNodeId, po.fromId);
                    if (path == null)
                    {
                        sw.WriteLine($"=========================== ERROR ===========================");
                        sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {po.fromId}");
                        sw.WriteLine($"=============================================================");
                        return;
                    }

                    vehicle.path = path.Nodes;
                    vehicle.pathIterator = 0;
                    vehicle.lPassed = 0;
                    vehicle.lastPointId = point.PointId;
                    ChangeState(vehicle, sw, VehicleState.MOVES_WITHOUT_LOAD);
                    return;
                }
            }

            var path2 = RouteCalculator.GetPathBetween(vehicle.lastNodeId, vehicle.startId);
            if (path2 == null)
            {
                sw.WriteLine($"=========================== ERROR ===========================");
                sw.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {vehicle.name}:{vehicle.number} can't move from node {vehicle.lastNodeId} to node {vehicle.startId}");
                sw.WriteLine($"=============================================================");
                return;
            }

            vehicle.path = path2.Nodes;
            vehicle.pathIterator = 0;
            vehicle.lPassed = 0;
            ChangeState(vehicle, sw, VehicleState.MOVES_BACK);
        }

        public void SaveStats()
        {
            StreamWriter sw = new("PERFORMANCE.TXT");
            sw.WriteLine($"Average load time: {performance.AverageLoadTime}");
            sw.WriteLine($"Average unload time: {performance.AverageUnloadTime}");
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

    }
}
