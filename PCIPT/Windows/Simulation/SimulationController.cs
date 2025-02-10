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
        private List<VehicleByRoutesRow> plan;

        private List<VehicleTypeDto> vehicleTypes;

        private List<VehicleObject> vehicleObjects = new();
        private List<PointObject> pointObjects = new();

        public SimulationController(List<VehicleByRoutesRow> plan, List<CargoTurnoverPointDto> pointsDto, List<VehicleTypeDto> vehicleTypes, List<CargoDto> cargoDtos, List<VehicleDto> vehicleDtos, List<GraphVehiclesData> vehiclesCoords, Dictionary<int, NegSize> nodesCoords, List<int> startPoints)
        {
            this.plan = plan;
            this.vehicleTypes = vehicleTypes;

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

                vehicleObjects.Add(new VehicleObject(dto.Coord.Width, dto.Coord.Height, dto.Type, vehicle.LoadCapacity, vehicle.LoadTime, vehicle.SpeedWithLoad, vehicle.SpeedWithoutLoad, vehicle.Type, dto.Coord, startPoints[i++], pairs[dto.Type]));
            }
        }

        public void NextStep(double deltaSeconds)
        {
            foreach (var vehicle in vehicleObjects)
            {
                switch (vehicle.vehicleState)
                {
                    case VehicleState.AWAITS:
                        {
                            var pointsForVehicle = plan.Where(p => p.Name == vehicle.name && p.Number == vehicle.number).ToList();
                            foreach (var point in pointsForVehicle)
                            {
                                var po = pointObjects.Where(po => po.pointId == point.PointId).First();
                                if (po.cargoLeft > 0)
                                {
                                    vehicle.pointId = po.pointId;
                                    vehicle.destination = new(po.fromX, po.fromY);
                                    vehicle.vehicleState = VehicleState.MOVES_WITHOUT_LOAD;
                                    break;
                                }
                            }
                            if (vehicle.pointId == -1)
                            {
                                vehicle.destination = vehicle.startPoint;
                                vehicle.vehicleState = VehicleState.MOVES_BACK;
                            }
                        }
                        break;
                    case VehicleState.MOVES_WITH_LOAD:
                        {
                            double directionX = vehicle.destination.Width - vehicle.X;
                            double directionY = vehicle.destination.Height - vehicle.Y;
                            double l = Math.Sqrt(directionX * directionX + directionY * directionY);

                            directionX /= l;
                            directionY /= l;

                            var po = pointObjects.Where(po => po.pointId == vehicle.pointId).First();

                            double deltaX = directionX * vehicle.speedWithCargo * deltaSeconds / po.distance;
                            double deltaY = directionY * vehicle.speedWithCargo * deltaSeconds / po.distance;

                            double deltaL = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                            if (deltaL >= l)
                            {
                                deltaL = l;
                                vehicle.loadTimeRemaining = vehicle.loadTime;
                                vehicle.vehicleState = VehicleState.UNLOADS;
                            }

                            vehicle.X += directionX * deltaL;
                            vehicle.Y += directionY * deltaL;
                        }
                        break;
                    case VehicleState.MOVES_WITHOUT_LOAD:
                        {
                            double directionX = vehicle.destination.Width - vehicle.X;
                            double directionY = vehicle.destination.Height - vehicle.Y;
                            double l = Math.Sqrt(directionX * directionX + directionY * directionY);

                            directionX /= l;
                            directionY /= l;

                            var po = pointObjects.Where(po => po.pointId == vehicle.pointId).First();

                            double deltaX = directionX * vehicle.speedWithoutCargo * deltaSeconds / po.distance;
                            double deltaY = directionY * vehicle.speedWithoutCargo * deltaSeconds / po.distance;

                            double deltaL = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                            if (deltaL >= l)
                            {
                                deltaL = l;
                                if (po.cargoLeft > 0f)
                                {
                                    po.cargoLeft -= vehicle.maxLoad * po.utilizationRate;
                                    vehicle.loadTimeRemaining = vehicle.loadTime;
                                    vehicle.vehicleState = VehicleState.LOADS;
                                }
                            }

                            vehicle.X += directionX * deltaL;
                            vehicle.Y += directionY * deltaL;
                        }
                        break;
                    case VehicleState.MOVES_BACK:
                        {
                            double directionX = vehicle.destination.Width - vehicle.X;
                            double directionY = vehicle.destination.Height - vehicle.Y;
                            double l = Math.Sqrt(directionX * directionX + directionY * directionY);

                            directionX /= l;
                            directionY /= l;

                            var po = pointObjects.Where(po => po.pointId == vehicle.pointId).First();
                            double len = RouteCalculator.GetDistanceBetween(po.toId, vehicle.startId);

                            double deltaX = directionX * vehicle.speedWithoutCargo * deltaSeconds / len;
                            double deltaY = directionY * vehicle.speedWithoutCargo * deltaSeconds / len;

                            double deltaL = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                            if (deltaL >= l)
                            {
                                deltaL = l;
                            }

                            vehicle.X += directionX * deltaL;
                            vehicle.Y += directionY * deltaL;
                        }
                        break;
                    case VehicleState.LOADS:
                        {
                            if (vehicle.loadTimeRemaining < 0)
                            {
                                vehicle.vehicleState = VehicleState.MOVES_WITH_LOAD;

                                var po = pointObjects.Where(po => po.pointId == vehicle.pointId).First();
                                if (po.cargoLeft > 0)
                                {
                                    vehicle.destination = new(po.toX, po.toY);
                                    vehicle.vehicleState = VehicleState.MOVES_WITH_LOAD;
                                }
                                else
                                {
                                    vehicle.vehicleState = VehicleState.AWAITS;
                                }
                            }
                            else vehicle.loadTimeRemaining -= deltaSeconds;
                        }
                        break;
                    case VehicleState.UNLOADS:
                        {
                            if (vehicle.loadTimeRemaining < 0)
                            {
                                vehicle.vehicleState = VehicleState.MOVES_WITHOUT_LOAD;

                                var po = pointObjects.Where(po => po.pointId == vehicle.pointId).First();
                                if (po.cargoLeft > 0)
                                {
                                    vehicle.destination = new(po.fromX, po.fromY);
                                    vehicle.vehicleState = VehicleState.MOVES_WITHOUT_LOAD;
                                }
                                else
                                {
                                    vehicle.vehicleState = VehicleState.AWAITS;
                                }
                            }
                            else vehicle.loadTimeRemaining -= deltaSeconds;
                        }
                        break;
                }
            }
        }

        public List<GraphVehiclesData> GetNewGraphVehiclesData()
        {
            List<GraphVehiclesData> graphVehiclesDatas = new();

            foreach (var vehicle in vehicleObjects)
            {
                graphVehiclesDatas.Add(new GraphVehiclesData(vehicle.name, new(vehicle.X, vehicle.Y)));
            }

            return graphVehiclesDatas;
        }

    }
}
