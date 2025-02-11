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
        OVER,
    }

    public sealed class VehicleObject
    {
        public NegSize bias;
        public int number;
        public List<int> path = new();
        public int pathIterator;
        public int lastNodeId;
        public double lPassed = 0;
        public double load = 0;
        public string name;
        public double maxLoad;
        public double loadTime;
        public double speedWithCargo;
        public double speedWithoutCargo;
        public string vehicleType;
        public int lastPointId;
        public VehicleState vehicleState = VehicleState.AWAITS;
        public double loadTimeRemaining;
        public int startId = -1;
        public bool dislocated = true;

        public VehicleObject(int number, string name, double maxLoad, double loadTime, double speedWithCargo, double speedWithoutCargo, string vehicleType, int startId, int lastNodeId, NegSize bias)
        {
            this.number = number;
            this.name = name;
            this.maxLoad = maxLoad;
            this.loadTime = loadTime;
            this.speedWithCargo = speedWithCargo;
            this.speedWithoutCargo = speedWithoutCargo;
            this.vehicleType = vehicleType;
            this.startId = startId;
            this.lastNodeId = lastNodeId;
            this.bias = bias;
        }
    }
}
