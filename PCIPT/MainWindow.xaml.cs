using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using PCIPT.Calculations.FirstStage.CargoStats;
using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Calculations.FirstStage.DefineRoutes;
using PCIPT.Calculations.FirstStage.FinalCostByVehicle;
using PCIPT.Calculations.FirstStage.LossByVehicle;
using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleInRoute;
using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows;

namespace PCIPT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var w = new GraphWindow();
            w.Show();
            //Test1();
            Test2();
        }

        public void Test1()
        {
            var costWeightDto = CsvHandler.GetAllFromFile<CostWeightDto>("CostWeightDto.csv");

            var vehicles = new List<VehicleDto>();

            var records = CsvHandler.GetAllFromFile<FuelVehicleDto>("FuelVehicles.csv");
            foreach (var record in records)
                vehicles.Add(record);

            var records2 = CsvHandler.GetAllFromFile<ElectricVehicleDto>("ElectricVehicles.csv");
            foreach (var record in records2)
                vehicles.Add(record);

            var ans = CostByVehicleCalculator.CalculateCostsByVehicles(vehicles, costWeightDto);
            CsvHandler.PutAllToFile("VehiclesCost.csv", ans.OrderBy(a => a.TotalLoss));
        }

        private record TestPath(int From, int To, float Distance);

        public void Test2()
        {
            float DAILY_TIME_FUND = 780f; // m
            int WORKING_DAYS = 14; // d
            float MAX_DAILY_CARGO = 350f; // t per day

            List<RouteDto> routes = CsvHandler.GetAllFromFile<RouteDto>("Routes.csv");
            List<CargoDto> cargoes = CsvHandler.GetAllFromFile<CargoDto>("Cargoes.csv");
            List<CargoTurnoverPointDto> points = CsvHandler.GetAllFromFile<CargoTurnoverPointDto>("CargoTurnoverPoints.csv");
            List<VehicleTypeDto> vehicleTypes = CsvHandler.GetAllFromFile<VehicleTypeDto>("VehicleTypes.csv");
            List<NodeDto> nodes = CsvHandler.GetAllFromFile<NodeDto>("Nodes.csv");

            RouteCalculator.InitRoutes(routes, nodes);

            List<TestPath> testPaths = new();
            for (int i = 1; i < nodes.Count + 1; i++)
            {
                for (int j = 1; j < nodes.Count + 1; j++)
                {
                    if (i == j) continue;
                    testPaths.Add(new TestPath(i, j, RouteCalculator.GetDistanceBetween(i, j)));
                }
            }
            CsvHandler.PutAllToFile("Paths.csv", testPaths);

            var vehicles = new List<VehicleDto>();

            var records = CsvHandler.GetAllFromFile<FuelVehicleDto>("FuelVehicles.csv");
            foreach (var record in records) vehicles.Add(record);

            var records2 = CsvHandler.GetAllFromFile<ElectricVehicleDto>("ElectricVehicles.csv");
            foreach (var record in records2) vehicles.Add(record);

            var costTable = CsvHandler.GetAllFromFile<CostTableRowEntity>("VehiclesCost.csv");

            Dictionary<string, float> vehicleRemains = new();
            foreach (var vehicle in vehicles)
            {
                vehicleRemains[vehicle.Name] = vehicle.MaxQuantity;
            }

            var vehiclesByRoutes = VehiclesByRoutesCalculator.CalculateVehicleStatsByPoints(points, routes, cargoes, vehicles, vehicleTypes, costTable, DAILY_TIME_FUND, WORKING_DAYS, MAX_DAILY_CARGO);
            foreach (var vehicleByRoutes in vehiclesByRoutes)
            {
                CsvHandler.PutAllToFile(vehicleByRoutes.Key.FileName, vehicleByRoutes.Value);
            }

            var distributedTasks = VehiclesByRoutesCalculator.DistributeTasksByVehicles(vehiclesByRoutes, vehicles, points, routes, DAILY_TIME_FUND);
            CsvHandler.PutAllToFile("DistributedTasks.csv", distributedTasks);

            var fuelVehicles = CsvHandler.GetAllFromFile<FuelVehicleDto>("FuelVehicles.csv");
            var electricVehicles = CsvHandler.GetAllFromFile<ElectricVehicleDto>("ElectricVehicles.csv");

            var finalCost = FinalCostCalculator.CalculateFinalCost(distributedTasks, fuelVehicles, electricVehicles, DAILY_TIME_FUND);
            CsvHandler.PutAllToFile("FinalCosts.csv", finalCost);
        }
    }
}
