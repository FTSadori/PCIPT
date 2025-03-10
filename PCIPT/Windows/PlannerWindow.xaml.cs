using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.DefineRoutes;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.LossByVehicle;
using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Core.DataHandler;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Vehicles;
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
        public PlannerWindow()
        {
            InitializeComponent();

            This = this;
        }

        public static PlannerWindow This;

        public void PerformPlanning()
        {
            string message1 = PrepareDataForPlanning();
            if (message1 == "")
            {
                string message2 = CalculatePathes();
                if (message2 == "")
                {
                    TestMessage.Text = "Success";
                }
                else
                    TestMessage.Text = message2;
            }
            else
                TestMessage.Text = message1;
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

        private void InputDataButton_Click(object sender, RoutedEventArgs e)
        {
            new PlannerImportWindow().Show();
        }
    }
}
