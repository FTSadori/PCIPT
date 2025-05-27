using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableCargoAssessObject : INotifyPropertyChanged
    {
        private int code;
        private string name = "";
        private float cargoTransported;
        private float persentageDone;

        public int Code
        {
            get => code;
            set { code = value; OnPropertyChanged(nameof(Code)); }
        }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public float CargoTransported
        {
            get { return cargoTransported; }
            set { cargoTransported = value; OnPropertyChanged(nameof(CargoTransported)); }
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
