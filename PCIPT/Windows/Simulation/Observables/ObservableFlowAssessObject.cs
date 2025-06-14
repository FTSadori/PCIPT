using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableFlowAssessObject : INotifyPropertyChanged
    {
        private int id;
        private string source = "";
        private string destination = "";
        private float cargoTransported;
        private float persentageDone;
        private float delay = 0;
        private string cargoName = "";

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Source
        {
            get => source;
            set { source = value.Replace('/', ' '); OnPropertyChanged(nameof(Source)); }
        }

        public string Destination
        {
            get => destination;
            set { destination = value.Replace('/', ' '); OnPropertyChanged(nameof(Destination)); }
        }

        public string CargoName
        {
            get => cargoName;
            set { cargoName = value; OnPropertyChanged(nameof(CargoName)); }
        }

        public float CargoTransported
        {
            get { return cargoTransported; }
            set { cargoTransported = value; OnPropertyChanged(nameof(CargoTransported)); }
        }

        public float Delay
        {
            get { return delay; }
            set { delay = value; OnPropertyChanged(nameof(Delay)); }
        }

        public float PersentageDone
        {
            get { return persentageDone; }
            set { persentageDone = value; OnPropertyChanged(nameof(PersentageDone)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
