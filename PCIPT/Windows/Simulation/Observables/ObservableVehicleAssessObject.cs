using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.Simulation.Observables
{
    public sealed class ObservableVehicleAssessObject : INotifyPropertyChanged
    {
        private int id;
        private string name = "";
        private int number;
        private float operatingTime = 0f;
        private float operatingTimePersentage = 0f;
        private float totalDelay = 0f;
        private float totalCargoTransported = 0f;
        private float workDonePersentage = 0f;
        private float requestsDone = 0f;
        private float requestsTotal = 0f;
        private float requestsDonePersentage = 0f;

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

        public float OperatingTime
        {
            get { return operatingTime; }
            set { operatingTime = value; OnPropertyChanged(nameof(OperatingTime)); }
        }

        public float OperatingTimePersentage
        {
            get { return operatingTimePersentage; }
            set { operatingTimePersentage = value; OnPropertyChanged(nameof(OperatingTimePersentage)); }
        }

        public float TotalCargoTransported
        {
            get { return totalCargoTransported; }
            set { totalCargoTransported = value; OnPropertyChanged(nameof(TotalCargoTransported)); }
        }

        public float WorkDonePersentage
        {
            get { return workDonePersentage; }
            set { workDonePersentage = value; OnPropertyChanged(nameof(WorkDonePersentage)); }
        }

        public float RequestsDone
        {
            get { return requestsDone; }
            set { requestsDone = value; OnPropertyChanged(nameof(RequestsDone)); }
        }

        public float RequestsTotal
        {
            get { return requestsTotal; }
            set { requestsTotal = value; OnPropertyChanged(nameof(RequestsTotal)); }
        }

        public float RequestsDonePersentage
        {
            get { return requestsDonePersentage; }
            set { requestsDonePersentage = value; OnPropertyChanged(nameof(RequestsDonePersentage)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
