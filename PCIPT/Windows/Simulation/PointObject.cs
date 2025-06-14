using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation
{
    public sealed class PointObject
    {
        public int pointId;
        public double cargoLeft;
        public double actualCargoLeft;
        public double allDailyCargo;
        public double fromX;
        public double fromY;
        public double toX;
        public double toY;
        public string cargoType;
        public double utilizationRate;
        public double distance;
        public int fromId;
        public int toId;
        public double dueTime;

        public PointObject(int pointId, double cargoLeft, double actualCargoLeft, double allDailyCargo, double fromX, double fromY, double toX, double toY, string cargoType, double utilizationRate, double distance, int fromId, int toId, double dueTime)
        {
            this.pointId = pointId;
            this.cargoLeft = cargoLeft;
            this.actualCargoLeft = actualCargoLeft;
            this.allDailyCargo = allDailyCargo;
            this.fromX = fromX;
            this.fromY = fromY;
            this.toX = toX;
            this.toY = toY;
            this.cargoType = cargoType;
            this.utilizationRate = utilizationRate;
            this.distance = distance;
            this.fromId = fromId;
            this.toId = toId;
            this.dueTime = dueTime;
        }
    }
}
