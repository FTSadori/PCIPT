using Microsoft.IdentityModel.Abstractions;
using Microsoft.Win32;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Core.DataHandler;
using PCIPT.Database.Commands;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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
    /// Логика взаимодействия для PlannerImportWindow.xaml
    /// </summary>
    public partial class PlannerImportWindow : Window
    {
        public PlannerImportWindow(DbContext dbContext)
        {
            InitializeComponent();

            DbContext = dbContext;
            ClearData();
        }

        DbContext DbContext { get; set; }

        private void ClearData()
        {
            costWeightDtos = null;
            fuelVehicleDtos = null;
            electricVehicleDtos = null;
            routeDtos = null;
            cargoDtos = null;
            cargoTurnoverPointDtos = null;
            vehicleTypeDtos = null;
            nodeDtos = null;
            vehicleDtos = null;
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

        public static int workingDays = 0;
        public static int workShifts = 0;
        public static float timeUsageFraction = 0;
        public static float timeFund = 0f;
        public static List<CostWeightDto>? costWeightDtos = null;
        public static List<FuelVehicleDto>? fuelVehicleDtos = null;
        public static List<ElectricVehicleDto>? electricVehicleDtos = null;
        public static List<RouteDto>? routeDtos = null;
        public static List<CargoDto>? cargoDtos = null;
        public static List<CargoTurnoverPointDto>? cargoTurnoverPointDtos = null;
        public static List<VehicleTypeDto>? vehicleTypeDtos = null;
        public static List<NodeDto>? nodeDtos = null;

        public static List<VehicleDto>? vehicleDtos = null;

        private void CheckIfPlanningIsAvaliable()
        {
            if (costWeightDtos != null && fuelVehicleDtos != null &&
                electricVehicleDtos != null && routeDtos != null &&
                cargoDtos != null && cargoTurnoverPointDtos != null &&
                vehicleTypeDtos != null && nodeDtos != null)
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
                        case "CostWeights":
                            costWeightDtos = null;
                            costWeightDtos = CsvHandler.GetAllFromFile<CostWeightDto>(openFileDialog.FileName);
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

            CheckIfPlanningIsAvaliable();
        }

        private void CsvButton1_Click(object sender, RoutedEventArgs e)
        {
            AskCSVImport("CostWeights", sender as Button);
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

        private void StartPlanningButton_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try { workingDays = int.Parse(WorkingDaysTextBox.Text); }
            catch (Exception) { WorkingDaysTextBox.Foreground = Brushes.Red; error = true; }

            try { timeFund = float.Parse(DailyTimeFundTextBox.Text); }
            catch (Exception) { DailyTimeFundTextBox.Foreground = Brushes.Red; error = true; }

            try { workShifts = int.Parse(WorkShiftsTextBox.Text); }
            catch (Exception) { WorkShiftsTextBox.Foreground = Brushes.Red; error = true; }

            try { timeUsageFraction = float.Parse(TimeUsageTextBox.Text); }
            catch (Exception) { TimeUsageTextBox.Foreground = Brushes.Red; error = true; }


            //try { maxDailyCargo = float.Parse(MaxDailyCargoTextBox.Text); }
            //catch (Exception) { MaxDailyCargoTextBox.Foreground = Brushes.Red; error = true; }

            if (error) return;

            vehicleDtos = new();

            foreach (var record in fuelVehicleDtos) vehicleDtos.Add(record);
            foreach (var record in electricVehicleDtos) vehicleDtos.Add(record);

            Close();
            PlannerWindow.This.PerformPlanning();
        }

        private void ImportDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                costWeightDtos = null;
                costWeightDtos = PlannerDataFromDatabaseConverter.ToCostWeight(new SelectAllCostWeightsCommand(DbContext.SqlConnection).Execute());
                Indicator1.Text = "Good";
                Indicator1.Foreground = Brushes.LightGreen;
            } catch(Exception) { Indicator1.Text = "Error"; Indicator1.Foreground = Brushes.Red; }
            try
            {
                fuelVehicleDtos = null;
                fuelVehicleDtos = PlannerDataFromDatabaseConverter.ToFuelVehicles(new SelectAllFuelVehiclesCommand(DbContext.SqlConnection).Execute());
                Indicator2.Text = "Good";
                Indicator2.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator2.Text = "Error"; Indicator2.Foreground = Brushes.Red; }
            try
            {
                electricVehicleDtos = null;
                electricVehicleDtos = PlannerDataFromDatabaseConverter.ToElectricVehicles(new SelectAllElectricVehiclesCommand(DbContext.SqlConnection).Execute());
                Indicator3.Text = "Good";
                Indicator3.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator3.Text = "Error"; Indicator3.Foreground = Brushes.Red; }
            try
            {
                routeDtos = null;
                routeDtos = PlannerDataFromDatabaseConverter.ToRoutes(new SelectAllRoutesCommand(DbContext.SqlConnection).Execute());
                Indicator4.Text = "Good";
                Indicator4.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator4.Text = "Error"; Indicator4.Foreground = Brushes.Red; }
            try
            {
                cargoDtos = null;
                cargoDtos = PlannerDataFromDatabaseConverter.ToCargoes(new SelectAllCargoesCommand(DbContext.SqlConnection).Execute());
                Indicator5.Text = "Good";
                Indicator5.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator5.Text = "Error"; Indicator5.Foreground = Brushes.Red; }
            try
            {
                cargoTurnoverPointDtos = null;
                cargoTurnoverPointDtos = PlannerDataFromDatabaseConverter.ToCargoTurnoverPoints(new SelectAllCargoTurnoverPointsCommand(DbContext.SqlConnection).Execute());
                Indicator6.Text = "Good";
                Indicator6.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator6.Text = "Error"; Indicator6.Foreground = Brushes.Red; }
            try
            {
                vehicleTypeDtos = null;
                vehicleTypeDtos = PlannerDataFromDatabaseConverter.ToVehicleTypes(new SelectAllVehicleTypesCommand(DbContext.SqlConnection).Execute());
                Indicator7.Text = "Good";
                Indicator7.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator7.Text = "Error"; Indicator7.Foreground = Brushes.Red; }
            try
            {
                nodeDtos = null;
                nodeDtos = PlannerDataFromDatabaseConverter.ToNodes(new SelectAllNodesCommand(DbContext.SqlConnection).Execute());
                Indicator8.Text = "Good";
                Indicator8.Foreground = Brushes.LightGreen;
            }
            catch (Exception) { Indicator8.Text = "Error"; Indicator8.Foreground = Brushes.Red; }

            CheckIfPlanningIsAvaliable();
        }
    }
}
