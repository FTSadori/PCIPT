using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PCIPT.Windows.Simulation
{
    public sealed class Performance
    {
        public double accumulatedLoadTime = 0;
        public int loadTimes = 0;
        public double AverageLoadTime => accumulatedLoadTime / Math.Max(1, loadTimes);

        public double accumulatedUnloadTime = 0;
        public int unloadTimes = 0;
        public double AverageUnloadTime => accumulatedUnloadTime / Math.Max(1, unloadTimes);

        public int requestsDone = 0;
        public double totalTimePassed = 0;
        public double Productivity => requestsDone / Math.Max(1, totalTimePassed);

        public int requestsDoneInTime = 0;
        public double Rhythmicity => requestsDoneInTime / Math.Max(1, requestsDone);

        public double accumulatedDelay = 0;
        public double AverageDelay => accumulatedDelay / Math.Max(1, requestsDone);

        public int requstsWithFullUtilization = 0;
        public double Unitization => requstsWithFullUtilization / Math.Max(1, requestsDone);

        public int requestsDoneAlmostGood = 0;
        public int RequestDoneBad => requestsDone - requestsDoneAlmostGood - requestsDoneInTime;
        public double QualityIndex => requestsDoneInTime + 0.5 * requestsDoneAlmostGood - RequestDoneBad;

        public double loss = 0;
    }
}
