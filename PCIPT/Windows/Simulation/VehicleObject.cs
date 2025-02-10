using PCIPT.Dtos.CargoTurnoverPoints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation
{
    public enum VehicleState
    {
        AWAITS,
        MOVES_WITH_LOAD,
        MOVES_WITHOUT_LOAD,
        MOVES_BACK,
        LOADS,
        UNLOADS,
    }

    public sealed class VehicleObject
    {
        public double X;
        public double Y;
        public NegSize? destination = null;
        public NegSize startPoint;
        public int number;
        public int pointId = -1;
        public double load = 0;
        public string name;
        public double maxLoad;
        public double loadTime;
        public double speedWithCargo;
        public double speedWithoutCargo;
        public string vehicleType;
        public VehicleState vehicleState = VehicleState.AWAITS;
        public double loadTimeRemaining;
        public int startId = -1;

        public VehicleObject(double x, double y, string name, double maxLoad, double loadTime, double speedWithCargo, double speedWithoutCargo, string vehicleType, NegSize startPoint, int startId, int number)
        {
            X = x;
            Y = y;
            this.name = name;
            this.maxLoad = maxLoad;
            this.loadTime = loadTime;
            this.speedWithCargo = speedWithCargo;
            this.speedWithoutCargo = speedWithoutCargo;
            this.vehicleType = vehicleType;
            this.startPoint = startPoint;
            this.startId = startId;
            this.number = number;
        }
    }
}
