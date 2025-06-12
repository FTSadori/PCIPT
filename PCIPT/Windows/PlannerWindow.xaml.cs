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
using System.Windows.Forms;
using System.IO;

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
            ErrorText.Text = "";
            TableMenuButton_Click(new object(), new());
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
                        dailyCargoTurnoverPoints,
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
                        "ShiftCargoTurnoverPoints.csv",
                        "VehicleCosts.csv",
                        "Pathes.csv",
                        "DistributedTasks.csv",
                        "FinalCosts.csv",
                    };

                    UpdateOutputText();

                    AbleOptionButtons(true);

                    TablesStack.Children.Clear();
                    for (int i = 0; i < tableNames.Count; ++i)
                    {
                        TablesStack.Children.Add(PlannerTableRowObjectCreator.GetObject($"{i + 1}. {tableNames[i]}", i, ShowTable));
                    }

                    ErrorText.Text = "";
                }
                else
                {
                    ErrorText.Text = "Error while " + message2;
                }
            }
            else
            {
                ErrorText.Text = "Error while " + message1;
            }
        }

        public void UpdateOutputText()
        {
            PlanText.Text = "";

            PlanText.Text += "Необхідна техніка:\n";
            foreach (var costLine in finalCost)
            {
                if (costLine.UsedNumber > 0)
                {
                    if (costLine.Name == "Total")
                    {
                        PlanText.Text += $"  Загалом: {costLine.UsedNumber} шт.\n";
                    }
                    else
                        PlanText.Text += $"- {costLine.Name} ({costLine.UsedNumber} шт.)\n";
                }
            }

            PlanText.Text += "\nВантажопотоки:\n";
            double totalDaily = 0;
            double totalMonthly = 0;
            double total = 0;
            foreach (var point in PlannerImportWindow.cargoTurnoverPointDtos)
            {
                var source = PlannerImportWindow.nodeDtos.Find(n => n.Id == point.SourceId);
                var dest = PlannerImportWindow.nodeDtos.Find(n => n.Id == point.DestinationId);
                var cargo = PlannerImportWindow.cargoDtos.Find(n => n.Code == point.CargoCode);

                PlanText.Text += $"- Потік \"{source.Name.Replace('/', ' ')}->{dest.Name.Replace('/', ' ')}\" ({cargo.Name}):\n";
                var daily = point.OutgoingCargo / PlannerImportWindow.workingDays;
                if (PlannerImportWindow.workingDays >= 30)
                {
                    PlanText.Text += $"  Обсяг:  \t{daily / PlannerImportWindow.workShifts:0}\t{daily:0}\t{daily * 30:0}\t{point.OutgoingCargo:0} (т)\n";
                    totalMonthly += daily * 30;
                }
                else
                {
                    PlanText.Text += $"  Обсяг:  \t{daily / PlannerImportWindow.workShifts:0}\t{daily:0}\t-\t{point.OutgoingCargo:0} (т)\n";
                }
                totalDaily += daily;
                total += point.OutgoingCargo;
            }
            PlanText.Text += $"  Загалом:\t{totalDaily / PlannerImportWindow.workShifts:0}\t{totalDaily:0}\t{totalMonthly:0}\t{total:0} (т)\n";

            PlanText.Text += "\nВитрати:\n";
            var totalLine = finalCost.Find(c => c.Name == "Total");
            PlanText.Text += $"- Витрати на електрику: {totalLine.BaseElectricityConsumption * PlannerImportWindow.workingDays * PlannerImportWindow.workShifts:0.00} грн\n";
            PlanText.Text += $"- Витрати на паливо: {totalLine.FuelConsumption * PlannerImportWindow.workingDays * PlannerImportWindow.workShifts:0.00} грн\n";
            PlanText.Text += $"- Витрати на ремонти: {totalLine.RepairCosts * PlannerImportWindow.workingDays * PlannerImportWindow.workShifts:0.00} грн\n";
            PlanText.Text += $"- Витрати на оливи: {(totalLine.HydraulicOilConsumption + totalLine.TransmissionOilConsumption + totalLine.MotorOilConsumption) * PlannerImportWindow.workingDays * PlannerImportWindow.workShifts:0.00} грн\n";
            PlanText.Text += $"  Загалом: {(totalLine.BaseElectricityConsumption + totalLine.FuelConsumption + totalLine.RepairCosts + totalLine.HydraulicOilConsumption + totalLine.TransmissionOilConsumption + totalLine.MotorOilConsumption) * PlannerImportWindow.workingDays * PlannerImportWindow.workShifts:0.00} грн\n";

            PlanText.Text += "\nЩозмінний план перевезень:\n";
            var spareList = distributedTasks.ToList();
            spareList.Sort((a, b) => $"{a.Name}[{a.Number}]".CompareTo($"{b.Name}[{b.Number}]"));
            string lastVehicle = "";
            int iterator = 1;
            foreach (var task in spareList)
            {
                if (task.CargoCode == -1)
                {
                    continue;
                }

                string thisVehicle = $"{task.Name} [{task.Number}]";
                if (thisVehicle != lastVehicle)
                {
                    lastVehicle = thisVehicle;
                    iterator = 0;
                    PlanText.Text += $"- {lastVehicle}:\n";
                }

                var point = PlannerImportWindow.cargoTurnoverPointDtos.Find(n => n.Id == task.PointId);
                var source = PlannerImportWindow.nodeDtos.Find(n => n.Id == point.SourceId);
                if (iterator == 0)
                {
                    PlanText.Text += $"{iterator++}. Знаходиться початково в пункті \"{source.Name.Replace('/', ' ')}\" (id {source.Id})\n";
                }
                var dest = PlannerImportWindow.nodeDtos.Find(n => n.Id == point.DestinationId);
                var cargo = PlannerImportWindow.cargoDtos.Find(n => n.Code == task.CargoCode);
                var veh = PlannerImportWindow.vehicleDtos.Find(n => n.Name == task.Name);

                PlanText.Text += $"{iterator++}. Виконує перевезення на потоці \"{source.Name.Replace('/', ' ')}->{dest.Name.Replace('/', ' ')}\" ({cargo.Name})\n";
                PlanText.Text += $"\tМає виконати {task.NumberOfCycles} циклів перевезень по {veh.LoadCapacity * cargo.CapacityUtilisationRate:0.00} т вантажу.\n\tЗагалом до {veh.LoadCapacity * cargo.CapacityUtilisationRate * task.NumberOfCycles:0.00} т вантажу.\n";
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
                ExportCurrentTable.Visibility = Visibility.Visible;

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
                ExportCurrentTable.Visibility = Visibility.Hidden;

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
        List<CargoTurnoverPointDto>? dailyCargoTurnoverPoints = null;

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
                var vehiclesByRoutes = VehiclesByRoutesCalculator.CalculateVehicleStatsByPoints(PlannerImportWindow.cargoTurnoverPointDtos, PlannerImportWindow.routeDtos, PlannerImportWindow.cargoDtos, PlannerImportWindow.vehicleDtos, PlannerImportWindow.vehicleTypeDtos, vehicleCosts, PlannerImportWindow.timeFund * PlannerImportWindow.timeUsageFraction, PlannerImportWindow.workingDays * PlannerImportWindow.workShifts);
                foreach (var vehicleByRoutes in vehiclesByRoutes)
                {
                    CsvHandler.PutAllToFile(vehicleByRoutes.Key.FileName, vehicleByRoutes.Value);
                }

                errorMessage = "Distributing tasks by vehicles";
                distributedTasks = VehiclesByRoutesCalculator.DistributeTasksByVehicles(vehiclesByRoutes, PlannerImportWindow.vehicleDtos, PlannerImportWindow.cargoTurnoverPointDtos, PlannerImportWindow.routeDtos, PlannerImportWindow.timeFund, PlannerImportWindow.timeUsageFraction, PlannerImportWindow.workingDays * PlannerImportWindow.workShifts);

                errorMessage = "Calculating final cost";
                finalCost = FinalCostCalculator.CalculateFinalCost(distributedTasks, PlannerImportWindow.fuelVehicleDtos, PlannerImportWindow.electricVehicleDtos, PlannerImportWindow.timeFund * PlannerImportWindow.timeUsageFraction);

                errorMessage = "Generating daily CTP";
                dailyCargoTurnoverPoints = new();
                foreach (var ctp in PlannerImportWindow.cargoTurnoverPointDtos)
                {
                    dailyCargoTurnoverPoints.Add(new CargoTurnoverPointDto(ctp.Id, ctp.SourceId, ctp.DestinationId, ctp.OutgoingCargo / PlannerImportWindow.workingDays / PlannerImportWindow.workShifts, ctp.CargoCode, ctp.DeliveryTime));
                }
            }
            catch (Exception ex)
            {
                return errorMessage + ": " + ex.Message;
            }
            return "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new AuthorizationWindow().Show();
        }

        private void ExportDataButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = SelectPathToExport();

            if (selectedPath == "") return;

            for (int i = 0; i < tables.Count; ++i)
            {
                Export(i, selectedPath);
            }
        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTable();
            PerformPlanning();
        }

        private void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
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
                case 5: PlannerImportWindow.cargoTurnoverPointDtos.Add(new CargoTurnoverPointDto(0,0,0,0,0,0)); break;
                case 6: PlannerImportWindow.vehicleTypeDtos.Add(new VehicleTypeDto("", "")); break;
                case 7: PlannerImportWindow.nodeDtos.Add(new NodeDto(0, "")); break;
                case 8: dailyCargoTurnoverPoints.Add(new CargoTurnoverPointDto(0, 0, 0, 0, 0, 0)); break;
                case 9: vehicleCosts.Add(new CostTableRowEntity("",0,0,0,0,0,0)); break;
                case 10: testPaths.Add(new TestPath(0,0,0)); break;
                case 11: distributedTasks.Add(new VehicleByRoutesRow("",0,0,0,0,0,0,0,0)); break;
                case 12: finalCost.Add(new FinalCostRowEntity("",0,0,0,0,0,0,0,0,0,0)); break;
            }
            CurrentDataGrid.ItemsSource = null;
            CurrentDataGrid.ItemsSource = tables[currentId];
        }

        private void ExportCurrentTable_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = SelectPathToExport();

            if (selectedPath == "") return;

            Export(currentId, selectedPath);
        }

        public static string SelectPathToExport()
        {
            string selectedPath = "";

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Оберіть папку для експорту";
                dialog.UseDescriptionForTitle = true; // Працює на нових Windows
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    selectedPath = dialog.SelectedPath;
                }
            }

            return selectedPath;
        }

        private void Export(int tableId, string selectedPath)
        {
            if (tableId == -1) return;
            switch (tableId)
            {
                case 0: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.costWeightDtos); break;
                case 1: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.fuelVehicleDtos); break;
                case 2: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.electricVehicleDtos); break;
                case 3: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.routeDtos); break;
                case 4: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.cargoDtos); break;
                case 5: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.cargoTurnoverPointDtos); break;
                case 6: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.vehicleTypeDtos); break;
                case 7: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], PlannerImportWindow.nodeDtos); break;
                case 8: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], dailyCargoTurnoverPoints); break;
                case 9: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], vehicleCosts); break;
                case 10: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], testPaths); break;
                case 11: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], distributedTasks); break;
                case 12: CsvHandler.PutAllToFile(selectedPath + "\\" + tableNames[tableId], finalCost); break;
            }
        }

        private void PlanMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PlanMenuText.Visibility = Visibility.Visible;
            PlanMenuButton.Visibility = Visibility.Hidden;
            TableMenuText.Visibility = Visibility.Hidden;
            TableMenuButton.Visibility = Visibility.Visible;
            TableMenuBorder.Visibility = Visibility.Hidden;
            PlanMenuBorder.Visibility = Visibility.Visible;
        }

        private void TableMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PlanMenuText.Visibility = Visibility.Hidden;
            PlanMenuButton.Visibility = Visibility.Visible;
            TableMenuText.Visibility = Visibility.Visible;
            TableMenuButton.Visibility = Visibility.Hidden;
            TableMenuBorder.Visibility = Visibility.Visible;
            PlanMenuBorder.Visibility = Visibility.Hidden;
        }

        private void ExportPlanButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = SelectPathToExport();

            if (selectedPath == "") return;

            StreamWriter sw = new(selectedPath + "\\plan.txt");
            sw.WriteLine(PlanText.Text);
            sw.Close();
        }

        private void InputDataButton_Click(object sender, RoutedEventArgs e)
        {
            new PlannerImportWindow(DbContext).ShowDialog();
        }
    }
}
