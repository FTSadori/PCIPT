using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.DefineRoutes;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.LossByVehicle;
using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows.ObjectCreators;
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
    /// Логика взаимодействия для PlannerWindow.xaml
    /// </summary>

    public partial class PlannerWindow : Window
    {
        public PlannerWindow(DbContext dbContext)
        {
            InitializeComponent();

            DbContext = dbContext;
            This = this;

            //CurrentDataGrid.ItemsSource = people;
            AbleOptionButtons(false);
        }

        DbContext DbContext { get; set; }

        public static PlannerWindow This;

        public int currentId = -1;
        public List<System.Collections.IEnumerable> tables = new();
        public List<string> tableNames = new();

        public void ShowTable(int id)
        {
            currentId = id;
            CurrentTableText.Text = $"Current: {tableNames[id]}";
            CurrentDataGrid.ItemsSource = tables[id];
        }

        public void ClearTable()
        {
            currentId = -1;
            CurrentTableText.Text = $"Current: Empty";
            CurrentDataGrid.ItemsSource = null;
        }

        public void PerformPlanning()
        {
            string message1 = PrepareDataForPlanning();
            if (message1 == "")
            {
                string message2 = CalculatePathes();
                if (message2 == "")
                {
                    tables = new()
                    {
                        PlannerImportWindow.costWeightDtos,
                        PlannerImportWindow.fuelVehicleDtos,
                        PlannerImportWindow.electricVehicleDtos,
                        PlannerImportWindow.routeDtos,
                        PlannerImportWindow.cargoDtos,
                        PlannerImportWindow.cargoTurnoverPointDtos,
                        PlannerImportWindow.vehicleTypeDtos,
                        PlannerImportWindow.nodeDtos,
                        vehicleCosts,
                        testPaths,
                        distributedTasks,
                        finalCost,
                    };
                    tableNames = new()
                    {
                        "CostWeights.csv",
                        "FuelVehicles.csv",
                        "ElectricVehicles.csv",
                        "Routes.csv",
                        "Cargoes.csv",
                        "CargoTurnoverPoints.csv",
                        "VehicleTypes.csv",
                        "Nodes.csv",
                        "VehicleCosts.csv",
                        "TextPathes.csv",
                        "DistributedTasks.csv",
                        "FinalConsts.csv",
                    };

                    AbleOptionButtons(true);

                    TablesStack.Children.Clear();
                    for (int i = 0; i < tableNames.Count; ++i)
                    {
                        TablesStack.Children.Add(PlannerTableRowObjectCreator.GetObject($"{i + 1}. {tableNames[i]}", i, ShowTable));
                    }
                }
            }
        }

        public void AbleOptionButtons(bool isDataPresent)
        {
            if (isDataPresent)
            {
                InputDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");
                ExportDataButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                RecalculateButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                ClearDataButton.SetResourceReference(BackgroundProperty, "ErrorGradient");

                AddRow.Visibility = Visibility.Visible;

                InputDataButton.IsEnabled = false;
                ExportDataButton.IsEnabled = true;
                RecalculateButton.IsEnabled = true;
                ClearDataButton.IsEnabled = true;
            }
            else
            {
                InputDataButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                ExportDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");
                RecalculateButton.SetResourceReference(BackgroundProperty, "DisableGradient");
                ClearDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                AddRow.Visibility = Visibility.Hidden;

                InputDataButton.IsEnabled = true;
                ExportDataButton.IsEnabled = false;
                RecalculateButton.IsEnabled = false;
                ClearDataButton.IsEnabled = false;
            }
        }

        private string PrepareDataForPlanning()
        {
            string errorMessage = "";

            try
            {
                errorMessage = "Calculating costs by vehicles";
                var ans = CostByVehicleCalculator.CalculateCostsByVehicles(PlannerImportWindow.vehicleDtos, PlannerImportWindow.costWeightDtos);

                errorMessage = "Creating VehiclesCost.csv file";
                vehicleCosts = ans.OrderBy(a => a.TotalLoss).ToList();
            }
            catch (Exception)
            {
                return errorMessage;
            }
            return "";
        }

        List<CostTableRowEntity>? vehicleCosts = null;
        List<TestPath>? testPaths = null;
        List<VehicleByRoutesRow>? distributedTasks = null;
        List<FinalCostRowEntity>? finalCost = null;

        private record TestPath(int From, int To, float Distance);

        private string CalculatePathes()
        {
            string errorMessage = "";

            try
            {
                errorMessage = "Initializing routes";
                RouteCalculator.InitRoutes(PlannerImportWindow.routeDtos, PlannerImportWindow.nodeDtos);

                testPaths = new();
                for (int i = 1; i < PlannerImportWindow.nodeDtos.Count + 1; i++)
                {
                    for (int j = 1; j < PlannerImportWindow.nodeDtos.Count + 1; j++)
                    {
                        if (i == j) continue;
                        testPaths.Add(new TestPath(i, j, RouteCalculator.GetDistanceBetween(i, j)));
                    }
                }

                Dictionary<string, float> vehicleRemains = new();
                foreach (var vehicle in PlannerImportWindow.vehicleDtos)
                {
                    vehicleRemains[vehicle.Name] = vehicle.MaxQuantity;
                }

                errorMessage = "Calculating vehicle stats by points";
                var vehiclesByRoutes = VehiclesByRoutesCalculator.CalculateVehicleStatsByPoints(PlannerImportWindow.cargoTurnoverPointDtos, PlannerImportWindow.routeDtos, PlannerImportWindow.cargoDtos, PlannerImportWindow.vehicleDtos, PlannerImportWindow.vehicleTypeDtos, vehicleCosts, PlannerImportWindow.timeFund, PlannerImportWindow.workingDays, PlannerImportWindow.maxDailyCargo);
                foreach (var vehicleByRoutes in vehiclesByRoutes)
                {
                    CsvHandler.PutAllToFile(vehicleByRoutes.Key.FileName, vehicleByRoutes.Value);
                }

                errorMessage = "Distributing tasks by vehicles";
                distributedTasks = VehiclesByRoutesCalculator.DistributeTasksByVehicles(vehiclesByRoutes, PlannerImportWindow.vehicleDtos, PlannerImportWindow.cargoTurnoverPointDtos, PlannerImportWindow.routeDtos, PlannerImportWindow.timeFund);

                errorMessage = "Calculating final cost";
                finalCost = FinalCostCalculator.CalculateFinalCost(distributedTasks, PlannerImportWindow.fuelVehicleDtos, PlannerImportWindow.electricVehicleDtos, PlannerImportWindow.timeFund);
            }
            catch (Exception)
            {
                return errorMessage;
            }
            return "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new AuthorizationWindow().Show();
        }

        private void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTable();
            PerformPlanning();
        }

        private void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Do you want to clear data?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                TablesStack.Children.Clear();
                ClearTable();
                AbleOptionButtons(false);
            }
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            if (currentId == -1) return;
            switch (currentId)
            {
                case 0: PlannerImportWindow.costWeightDtos.Add(new CostWeightDto("", 1)); break;
                case 1: PlannerImportWindow.fuelVehicleDtos.Add(new FuelVehicleDto("", "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0)); break;
                case 2: PlannerImportWindow.electricVehicleDtos.Add(new ElectricVehicleDto()); break;
                case 3: PlannerImportWindow.routeDtos.Add(new RouteDto(0, 0, 0, 0)); break;
                case 4: PlannerImportWindow.cargoDtos.Add(new CargoDto(0,"","",0)); break;
                case 5: PlannerImportWindow.cargoTurnoverPointDtos.Add(new CargoTurnoverPointDto(0,0,0,0,0)); break;
                case 6: PlannerImportWindow.vehicleTypeDtos.Add(new VehicleTypeDto("", "")); break;
                case 7: PlannerImportWindow.nodeDtos.Add(new NodeDto(0, "")); break;
                case 8: vehicleCosts.Add(new CostTableRowEntity("",0,0,0,0,0,0)); break;
                case 9: testPaths.Add(new TestPath(0,0,0)); break;
                case 10: distributedTasks.Add(new VehicleByRoutesRow("",0,0,0,0,0,0,0,0)); break;
                case 11: finalCost.Add(new FinalCostRowEntity("",0,0,0,0,0,0,0,0,0)); break;
            }
            CurrentDataGrid.ItemsSource = null;
            CurrentDataGrid.ItemsSource = tables[currentId];
        }

        private void InputDataButton_Click(object sender, RoutedEventArgs e)
        {
            new PlannerImportWindow(DbContext).ShowDialog();
        }
    }
}
