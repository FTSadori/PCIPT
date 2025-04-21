using Microsoft.Win32;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Graph;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PCIPT.Windows
{
    /// <summary>
    /// Логика взаимодействия для DispatcherImportWindow.xaml
    /// </summary>
    public partial class DispatcherImportWindow : Window
    {
        public DispatcherImportWindow(DbContext dbContext)
        {
            InitializeComponent();

            DbContext = dbContext;
            ClearData();
        }

        DbContext DbContext { get; set; }

        private void ClearData()
        {
            timeFund = 0f;
            nodesCoordsDtos = null;
            fuelVehicleDtos = null;
            electricVehicleDtos = null;
            routeDtos = null;
            cargoDtos = null;
            cargoTurnoverPointDtos = null;
            vehicleTypeDtos = null;
            nodeDtos = null;
            vehicleDtos = null;
            distributedTasks = null;
        }

        public static float timeFund = 0f;
        public static List<NodesCoordsDto>? nodesCoordsDtos = null;
        public static List<FuelVehicleDto>? fuelVehicleDtos = null;
        public static List<ElectricVehicleDto>? electricVehicleDtos = null;
        public static List<RouteDto>? routeDtos = null;
        public static List<CargoDto>? cargoDtos = null;
        public static List<CargoTurnoverPointDto>? cargoTurnoverPointDtos = null;
        public static List<VehicleTypeDto>? vehicleTypeDtos = null;
        public static List<NodeDto>? nodeDtos = null;

        public static List<VehicleDto>? vehicleDtos = null;


        public static List<VehicleByRoutesRow>? distributedTasks = null;

        private void ByAutomaticButton_Click(object sender, RoutedEventArgs e)
        {
            AutomaticGrid.Visibility = Visibility.Visible;
            PlanGrid.Visibility = Visibility.Collapsed;
            ByAutomaticButton.Visibility = Visibility.Hidden;
            ByAutomaticLabel.Visibility = Visibility.Visible;
            ByPlanButton.Visibility = Visibility.Visible;
            ByPlanLabel.Visibility = Visibility.Hidden;
            distributedTasks = null;
            ImportDistributed.SetResourceReference(Button.ForegroundProperty, "RoundedTextBoxGrad");
            ImportDistributed.SetResourceReference(Button.BorderBrushProperty, "RoundedTextBoxGrad");
            CheckIfApplyingIsAvaliable();
        }

        private void ByPlanButton_Click(object sender, RoutedEventArgs e)
        {
            AutomaticGrid.Visibility = Visibility.Collapsed;
            PlanGrid.Visibility = Visibility.Visible;
            ByAutomaticButton.Visibility = Visibility.Visible;
            ByAutomaticLabel.Visibility = Visibility.Hidden;
            ByPlanButton.Visibility = Visibility.Hidden;
            ByPlanLabel.Visibility = Visibility.Visible;
            CheckIfApplyingIsAvaliable();
        }

        private void CsvButton_Click(object sender, RoutedEventArgs e)
        {
            BorderCsv.Visibility = Visibility.Visible;
            BorderDatabase.Visibility = Visibility.Collapsed;

            CsvButton.Visibility = Visibility.Hidden;
            CsvSelected.Visibility = Visibility.Visible;

            DatabaseButton.Visibility = Visibility.Visible;
            DatabaseSelected.Visibility = Visibility.Hidden;
        }

        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            BorderCsv.Visibility = Visibility.Collapsed;
            BorderDatabase.Visibility = Visibility.Visible;

            CsvButton.Visibility = Visibility.Visible;
            CsvSelected.Visibility = Visibility.Hidden;

            DatabaseButton.Visibility = Visibility.Hidden;
            DatabaseSelected.Visibility = Visibility.Visible;
        }

        private void AskCSVImport(string category, Button sender)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a File",
                Filter = "Csv files (*.csv)|*.csv",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    switch (category)
                    {
                        case "NodesCoords":
                            nodesCoordsDtos = null;
                            nodesCoordsDtos = CsvHandler.GetAllFromFile<NodesCoordsDto>(openFileDialog.FileName);
                            break;
                        case "FuelVehicles":
                            fuelVehicleDtos = null;
                            fuelVehicleDtos = CsvHandler.GetAllFromFile<FuelVehicleDto>(openFileDialog.FileName);
                            break;
                        case "ElectricVehicles":
                            electricVehicleDtos = null;
                            electricVehicleDtos = CsvHandler.GetAllFromFile<ElectricVehicleDto>(openFileDialog.FileName);
                            break;
                        case "Routes":
                            routeDtos = null;
                            routeDtos = CsvHandler.GetAllFromFile<RouteDto>(openFileDialog.FileName);
                            break;
                        case "Cargoes":
                            cargoDtos = null;
                            cargoDtos = CsvHandler.GetAllFromFile<CargoDto>(openFileDialog.FileName);
                            break;
                        case "CargoTurnoverPoints":
                            cargoTurnoverPointDtos = null;
                            cargoTurnoverPointDtos = CsvHandler.GetAllFromFile<CargoTurnoverPointDto>(openFileDialog.FileName);
                            break;
                        case "VehicleTypes":
                            vehicleTypeDtos = null;
                            vehicleTypeDtos = CsvHandler.GetAllFromFile<VehicleTypeDto>(openFileDialog.FileName);
                            break;
                        case "Nodes":
                            nodeDtos = null;
                            nodeDtos = CsvHandler.GetAllFromFile<NodeDto>(openFileDialog.FileName);
                            break;
                        case "DistributedTasks":
                            distributedTasks = null;
                            distributedTasks = CsvHandler.GetAllFromFile<VehicleByRoutesRow>(openFileDialog.FileName);
                            break;
                    }

                    sender.Foreground = Brushes.LightGreen;
                    sender.BorderBrush = Brushes.LightGreen;
                }
                catch (Exception)
                {
                    sender.Foreground = Brushes.Red;
                    sender.BorderBrush = Brushes.Red;
                }
            }

            CheckIfApplyingIsAvaliable();
        }

        private void CsvButton1_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("NodesCoords", sender as Button);
        }

        private void CsvButton2_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("FuelVehicles", sender as Button);
        }

        private void CsvButton3_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("ElectricVehicles", sender as Button);
        }

        private void CsvButton4_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("Routes", sender as Button);
        }

        private void CsvButton5_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("Cargoes", sender as Button);
        }

        private void CsvButton6_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("CargoTurnoverPoints", sender as Button);
        }

        private void CsvButton7_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("VehicleTypes", sender as Button);
        }

        private void CsvButton8_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("Nodes", sender as Button);
        }

        private void CheckIfApplyingIsAvaliable()
        {
            if (nodesCoordsDtos != null && fuelVehicleDtos != null &&
                electricVehicleDtos != null && routeDtos != null &&
                cargoDtos != null && cargoTurnoverPointDtos != null &&
                vehicleTypeDtos != null && nodeDtos != null &&
                distributedTasks != null)
            {
                StartPlanningButton.IsEnabled = true;
                StartPlanningButton.SetResourceReference(Button.BackgroundProperty, "RoundedTextBoxGrad");
            }
            else
            {
                StartPlanningButton.IsEnabled = false;
                StartPlanningButton.SetResourceReference(Button.BackgroundProperty, "DisableGradient");
            }
        }

        private void ImportDistributed_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("DistributedTasks", sender as Button);
        }

        private void StartPlanningButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
