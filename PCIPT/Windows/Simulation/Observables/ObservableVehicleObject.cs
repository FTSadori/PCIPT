using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableVehicleObject : INotifyPropertyChanged
    {
        private int id;
        private string name = "";
        private int number;
        private VehicleState vehicleState;
        private int pointId;
        private int lastNodeId;
        private float currentLoad;
        private float currentSpeed;
        private float timeSinceLastStateChange;
        
        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int Number
        {
            get => number;
            set { number = value; OnPropertyChanged(nameof(Number)); }
        }

        public VehicleState VehicleState
        {
            get { return vehicleState; }
            set { vehicleState = value; OnPropertyChanged(nameof(VehicleState)); }
        }

        public int PointId
        {
            get { return pointId; }
            set { pointId = value; OnPropertyChanged(nameof(PointId)); }
        }

        public int LastNodeId
        {
            get { return lastNodeId; }
            set { lastNodeId = value; OnPropertyChanged(nameof(LastNodeId)); }
        }

        public float CurrentLoad
        {
            get { return currentLoad; }
            set { currentLoad = value; OnPropertyChanged(nameof(CurrentLoad)); }
        }

        public float CurrentSpeed
        {
            get { return currentSpeed; }
            set { currentSpeed = value; OnPropertyChanged(nameof(CurrentSpeed)); }
        }

        public float TimeSinceLastStateChange
        {
            get { return timeSinceLastStateChange; }
            set { timeSinceLastStateChange = value; OnPropertyChanged(nameof(TimeSinceLastStateChange)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
