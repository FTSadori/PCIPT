using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservablePointObject : INotifyPropertyChanged
    {
        private int id;
        private string source = "";
        private string destination = "";
        private float cargoLeft;
        private float fractionLeft;
        private string cargoName = "";

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Source
        {
            get => source;
            set { source = value; OnPropertyChanged(nameof(Source)); }
        }

        public string Destination
        {
            get => destination;
            set { destination = value; OnPropertyChanged(nameof(Destination)); }
        }

        public float CargoLeft
        {
            get { return cargoLeft; }
            set { cargoLeft = value; OnPropertyChanged(nameof(CargoLeft)); }
        }

        public float FractionLeft
        {
            get { return fractionLeft; }
            set { fractionLeft = value; OnPropertyChanged(nameof(FractionLeft)); }
        }

        public string CargoName
        {
            get => cargoName;
            set { cargoName = value; OnPropertyChanged(nameof(CargoName)); }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
