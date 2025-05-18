using PCIPT.Calculations.FirstStage.RouteFinder;
using PCIPT.Calculations.FirstStage.VehicleByRoutes.Dtos;
using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.Graph;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using PCIPT.Windows.DataHandlers;
using PCIPT.Windows.ObjectCreators;
using PCIPT.Windows.Simulation;
using PCIPT.Windows.Simulation.Observables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace PCIPT.Windows
{
    public record NegSize(double Width, double Height);

    /// <summary>
    /// Логика взаимодействия для GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window
    {
        public static GraphWindow This;

        public static void DoCmd(ThreadStart th)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, th);
        }

        private Dictionary<int, NegSize> nodesCoords = new();
        private List<GraphVehiclesData> vehiclesCoords = new();
        private List<NegSize> realNodesSize = new();

        private Dictionary<string, int> starts;
        private List<NegSize> biases;

        public List<NodeDto> Nodes;
        public List<RouteDto> Routes;
        public List<VehicleByRoutesRow> DistributedTasks;
        public List<VehicleByRoutesRow> DistributedTasksShort;
        public List<NodesCoordsDto> NodesCoords;
        public List<CargoTurnoverPointDto> CargoPoints;
        public List<VehicleTypeDto> VehicleTypes;
        public List<CargoDto> Cargoes;
        public List<VehicleDto> Vehicles;
        public double dailyTimeFund;

        private double GUISize = 1;
        private double CurrentSize = 1;
        private bool Init = false;
        private double TotalShiftHeight = 0;
        private double TotalShiftWidth = 0;

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
                AbleOptionButtons(false);

                TasksDataGrid.ItemsSource = null;
                Vehicles = null;
                Cargoes = null;
                VehicleTypes = null;
                CargoPoints = null;
                NodesCoords = null;
                DistributedTasks = null;
                Routes = null;
                Nodes = null;
                starts = null;
                biases = null;
                nodesCoords = new();
                vehiclesCoords = new();
                realNodesSize = new();

                Init = false;
                TotalShiftHeight = 0;
                TotalShiftWidth = 0;
                CurrentSize = 1;

                GraphCanvas.Children.Clear();
                VehiclesGraphCanvas.Children.Clear();
            }
        }

        public GraphWindow(DbContext dbContext)
        {
            InitializeComponent();

            This = this;

            DbContext = dbContext;

            AbleOptionButtons(false);
        }

        DbContext DbContext { get; set; }

        private void InitEverything(List<VehicleByRoutesRow> distributedTasks)
        {
            InitGraph();
            InitVehicles(distributedTasks);
        }

        public void AbleControlButton(bool isSimulationRunning)
        {
            if (isSimulationRunning)
            {
                ReroutingButton.IsEnabled = true;
                ReroutingButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");

                StopButton.IsEnabled = true;
                StopButton.SetResourceReference(BackgroundProperty, "ErrorGradient");

                PauseButton.IsEnabled = true;
                PauseButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");

                StartButton.IsEnabled = false;
                StartButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                ClearDataButton.IsEnabled = false;
                ClearDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");
            }
            else
            {
                ReroutingButton.IsEnabled = false;
                ReroutingButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                StopButton.IsEnabled = false;
                StopButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                PauseButton.IsEnabled = false;
                PauseButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                StartButton.IsEnabled = true;
                StartButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");

                ClearDataButton.IsEnabled = true;
                ClearDataButton.SetResourceReference(BackgroundProperty, "ErrorGradient");
            }
        }

        public void AbleOptionButtons(bool isDataPresent)
        {
            if (isDataPresent)
            {
                ImportDataButton.IsEnabled = false;
                ImportDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                ClearDataButton.IsEnabled = true;
                ClearDataButton.SetResourceReference(BackgroundProperty, "ErrorGradient");

                MenuButtonsStack.Visibility = Visibility.Visible;
                ControlStack.Visibility = Visibility.Visible;
                SettingsStack.Visibility = Visibility.Visible;
                TopPanelClosed.Visibility = Visibility.Visible;

                AbleControlButton(false);
            }
            else
            {
                ImportDataButton.IsEnabled = true;
                ImportDataButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");

                ClearDataButton.IsEnabled = false;
                ClearDataButton.SetResourceReference(BackgroundProperty, "DisableGradient");

                MenuButtonsStack.Visibility = Visibility.Hidden;
                ControlStack.Visibility = Visibility.Hidden;
                SettingsStack.Visibility = Visibility.Hidden;
                TopPanelClosed.Visibility = Visibility.Hidden;
                TopPanelOpened.Visibility = Visibility.Hidden;
            }
        }

        private void InitGraph()
        {
            nodesCoords = NormalizedCoordsConventer.ConvertFromOtherCoords(NodesCoords);

            Init = true;
        }

        private void ShiftGraph(NegSize Offset)
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is Grid grid)
                {
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) + Offset.Width);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) + Offset.Height);
                }
                else if (child is Line line)
                {
                    line.Y1 -= Offset.Height;
                    line.Y2 -= Offset.Height;
                    line.X1 += Offset.Width;
                    line.X2 += Offset.Width;
                }
            }

            ShiftOnlyVehicles(Offset);
        }

        private void ShiftOnlyVehicles(NegSize Offset)
        {
            foreach (var child in VehiclesGraphCanvas.Children)
            {
                if (child is Ellipse ellipse)
                {
                    Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + Offset.Width);
                    Canvas.SetBottom(ellipse, Canvas.GetBottom(ellipse) + Offset.Height);
                }
            }
        }

        private void AddToTotalOffset(NegSize Offset)
        {
            TotalShiftHeight += Offset.Height;
            TotalShiftWidth += Offset.Width;
        }

        private void AutoNormalizeElementsOnGraph(double Size)
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is Grid grid)
                {
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) - grid.ActualWidth / 2);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) - grid.ActualHeight / 2);
                    realNodesSize.Add(new NegSize(grid.ActualWidth / 2 / Size, grid.ActualHeight / 2 / Size));
                    grid.Visibility = Visibility.Visible;
                }
            }
        }

        private void NormalizeElementsOnGraph(double Size)
        {
            int i = 0;
            foreach (var child in GraphCanvas.Children)
            {
                if (child is Grid grid)
                {
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) - realNodesSize[i].Width * Size * GUISize);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) - realNodesSize[i].Height * Size * GUISize);
                    grid.Visibility = Visibility.Visible;
                    ++i;
                }
            }
        }

        private void InitVehicles(List<VehicleByRoutesRow> distributedTasks)
        {
            vehiclesCoords = GraphVehicleDataConverter.InitConvert(distributedTasks, nodesCoords, CargoPoints, out starts, out biases);
        }

        private void RenderVehicles(double size)
        {
            VehiclesGraphCanvas.Children.Clear();

            foreach (var child in AllVehicleNodesObjectCreator.GetObjects(GraphVehicleDataConverter.ConvertToScreenValues(vehiclesCoords, VehiclesGraphCanvas, size), size * GUISize))
            {
                VehiclesGraphCanvas.Children.Add(child);
            }
        }

        private void RenderGraph(List<NodeDto> nodes, List<RouteDto> routes, double size, NegSize shift)
        {
            GraphCanvas.Children.Clear();

            double NormalDistance = Math.Min(GraphCanvas.ActualWidth, GraphCanvas.ActualHeight) / 2.0 * 0.8 * size;

            NegSize zeroPoint = new(GraphCanvas.ActualWidth / 2 * size, GraphCanvas.ActualHeight / 2 * size);
            
            foreach (var route in routes)
            {
                NegSize realPoint1 = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[route.SourceId].Width,
                    GraphCanvas.ActualHeight - zeroPoint.Height - NormalDistance * nodesCoords[route.SourceId].Height);

                NegSize realPoint2 = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[route.DestinationId].Width,
                    GraphCanvas.ActualHeight - zeroPoint.Height - NormalDistance * nodesCoords[route.DestinationId].Height);

                GraphCanvas.Children.Add(CreateLine(realPoint1, realPoint2, size * GUISize));
            }

            foreach (var node in nodes) 
            {
                NegSize realPoint = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[node.Id].Width,
                    zeroPoint.Height + NormalDistance * nodesCoords[node.Id].Height);

                var nodeObj = CreateNodeGrid(node.Name, node.Id, realPoint, size * GUISize);
                nodeObj.Visibility = Visibility.Hidden;
                GraphCanvas.Children.Add(nodeObj);
            }

            ShiftGraph(shift);

            if (realNodesSize.Count == 0)
            {
                renderThread = new(delegate ()
                {
                    Thread.Sleep(50);
                    DoCmd(delegate () { AutoNormalizeElementsOnGraph(size); });
                });
                renderThread.IsBackground = true;
                renderThread.Start();
            }
            else NormalizeElementsOnGraph(size);
        }

        private Line CreateLine(NegSize From, NegSize To, double Size)
        {
            Line line = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 3.0 * Size,

                X1 = From.Width,
                X2 = To.Width,

                Y1 = From.Height,
                Y2 = To.Height,
            };

            return line;
        }

        private Grid CreateNodeGrid(string Name, int Id, NegSize Coord, double Size)
        {
            Grid node = new()
            {
                Background = Brushes.Black,
            };

            StackPanel stack = new();
            stack.Background = Brushes.White;
            stack.Margin = new Thickness(3 * Size);

            if (Name != "")
            {
                stack.Children.Add(new TextBlock()
                {
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(5 * Size),
                    Margin = new Thickness(0, 0, 0, 1 * Size),
                    FontSize = 16 * Size,
                    Background = Brushes.White,
                    Text = Name.Replace('/', '\n'),
                });

                stack.Children.Add(new TextBlock()
                {
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(3 * Size),
                    FontSize = 12 * Size,
                    Background = Brushes.White,
                    Text = "ID: " + Id.ToString(),
                });
            }
            else
            {
                stack.Width = 20 * Size;
                stack.Height = 20 * Size;
            }

            node.Children.Add(stack);

            Canvas.SetLeft(node, Coord.Width);
            Canvas.SetBottom(node, Coord.Height);

            return node;
        }

        Thread? renderThread = null;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rerender();
        }

        private void Rerender()
        {
            if (Init)
            {
                double width = GraphCanvas.ActualWidth;
                double height = GraphCanvas.ActualHeight;

                double sizeShiftX = width * CurrentSize / 2 - width / 2;
                double sizeShiftY = height * CurrentSize / 2 - height / 2;

                RenderVehicles(CurrentSize);
                RenderGraph(Nodes, Routes, CurrentSize, new NegSize(TotalShiftWidth - sizeShiftX, TotalShiftHeight - sizeShiftY));

                if (simulationThread != null)
                {
                    vehiclesCoords = simulationController.GetNewGraphVehiclesData();
                    RenderVehicles(CurrentSize);
                }
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            AddToTotalOffset(new(20, 0));
            ShiftGraph(new(20, 0));
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            AddToTotalOffset(new(-20, 0));
            ShiftGraph(new(-20, 0));
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            AddToTotalOffset(new(0, -20));
            ShiftGraph(new(0, -20));
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            AddToTotalOffset(new(0, 20));
            ShiftGraph(new(0, 20));
        }

        private void SizePlus_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize += 0.1;
            Rerender();

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;

            double sizeShiftX = width * CurrentSize / 2 - width / 2;
            double sizeShiftY = height * CurrentSize / 2 - height / 2;

            RenderVehicles(CurrentSize);
            ShiftOnlyVehicles(new NegSize(TotalShiftWidth - sizeShiftX, TotalShiftHeight - sizeShiftY));
        }

        private void SizeMinus_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize = Math.Max(CurrentSize - 0.1, 0.1);
            Rerender();

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;

            double sizeShiftX = width * CurrentSize / 2 - width / 2;
            double sizeShiftY = height * CurrentSize / 2 - height / 2;

            RenderVehicles(CurrentSize);
            ShiftOnlyVehicles(new NegSize(TotalShiftWidth - sizeShiftX, TotalShiftHeight - sizeShiftY));
        }

        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize = 1;
            TotalShiftHeight = 0;
            TotalShiftWidth = 0;
            Rerender();
        }

        Thread? simulationThread = null;

        int simulationSpeed = 1;

        double timeSpent = 0;

        int frameRate = 5;

        bool simPaused = false;
        bool readyToStop = false;

        private void StartSimulation()
        {
            if (File.Exists("LOG.TXT"))
            {
                File.Delete("LOG.TXT");
            }

            simulationThread = new(delegate ()
            {
                Stopwatch stopwatch = new();
                while (true)
                {
                    if (readyToStop)
                    {
                        readyToStop = false;
                        break;
                    }

                    if (simPaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    double additionTime = 0;

                    double width = GraphCanvas.ActualWidth;
                    double height = GraphCanvas.ActualHeight;

                    double sizeShiftX = width * CurrentSize / 2 - width / 2;
                    double sizeShiftY = height * CurrentSize / 2 - height / 2;

                    bool ended = false;

                    double timeElapsed = timeSpent + 1.0 / frameRate * simulationSpeed + additionTime;

                    DoCmd(delegate () {
                        stopwatch.Restart();

                        SetTimeSpent(timeSpent + 1.0 / frameRate * simulationSpeed + additionTime);
                        
                        simulationController.NextStep(1.0 / frameRate * simulationSpeed + additionTime, 1);
                        additionTime = 0;

                        vehiclesCoords = simulationController.GetNewGraphVehiclesData();
                        RenderVehicles(CurrentSize);

                        ShiftOnlyVehicles(new NegSize(TotalShiftWidth - sizeShiftX, TotalShiftHeight - sizeShiftY));
                        stopwatch.Stop();
                        ended = true;
                    });
                    int sleeped = 0;
                    while (!ended) { Thread.Sleep(1); sleeped += 1; }

                    int total = (int)(1.0 / frameRate * 1000 - stopwatch.Elapsed.TotalMilliseconds - sleeped);
                    DoCmd(delegate ()
                    {
                        PausedText.Visibility = Visibility.Visible;
                        PausedText.Text = $"{total} | {timeElapsed:0.00} s";
                    });

                    if (total <= 0)
                    {
                        additionTime = -total;
                    }
                    else
                        Thread.Sleep(total);
                }
            });
            simulationThread.IsBackground = true;
            simulationThread.Start();
        }

        SimulationController simulationController;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.K:
                    simulationController.SaveStats();
                    break;
                case Key.Down:
                    Down_Click(sender, new());
                    break;
                case Key.Up:
                    Up_Click(sender, new());
                    break;
                case Key.Left:
                    Left_Click(sender, new());
                    break;
                case Key.Right:
                    Right_Click(sender, new());
                    break;
                case Key.Z:
                    SizeMinus_Click(sender, new());
                    break;
                case Key.X:
                    SizePlus_Click(sender, new());
                    break;
            }
        }

        public void InitAndRender()
        {
            InitEverything(DistributedTasks);
            RenderVehicles(CurrentSize);
            RenderGraph(Nodes, Routes, CurrentSize, new NegSize(TotalShiftWidth, TotalShiftHeight));

            AbleOptionButtons(true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new AuthorizationWindow().Show();
        }


        private void InputDataButton_Click(object sender, RoutedEventArgs e)
        {
            new DispatcherImportWindow(DbContext).ShowDialog();
        }

        private void TopPanelClosedButton_Click(object sender, RoutedEventArgs e)
        {
            TopPanelClosed.Visibility = Visibility.Collapsed;
            TopPanelOpened.Visibility = Visibility.Visible;
        }

        private void TopPanelOpenedButton_Click(object sender, RoutedEventArgs e)
        {
            TopPanelClosed.Visibility = Visibility.Visible;
            TopPanelOpened.Visibility = Visibility.Collapsed;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // specify log file location

            OutputText.Text = "";

            ToObservableListTranslator.ObservableVehicles = new();
            ToObservablePointsListTranslator.ObservablePoints = new();
            VehiclesDataGrid.ItemsSource = ToObservableListTranslator.ObservableVehicles;
            PointsDataGrid.ItemsSource = ToObservablePointsListTranslator.ObservablePoints;
            ToObservablePointsListTranslator.nodeDtos = Nodes;
            ToObservablePointsListTranslator.cargoDtos = Cargoes;
            ToObservablePointsListTranslator.cargoTurnoverPointDtos = CargoPoints;

            RouteCalculator.InitRoutes(Routes, Nodes);
            simulationController = new(DistributedTasksShort, CargoPoints, VehicleTypes, Cargoes, Vehicles, vehiclesCoords, nodesCoords, starts, biases, "LOG.TXT", OutputText);
            StartSimulation();
            SetTimeSpent(0);

            AbleControlButton(true);
        }

        private void SetTimeSpent(double time)
        {
            timeSpent = time;
            TimePassedLabel.Text = $"{(int)(time / 3600)}h {(int)(time / 60 % 60)}m {(int)(time % 60)}s ({time / dailyTimeFund / 60 * 100:0.00}%)";
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (simPaused)
            {
                PauseButton.Content = "Pause";
                PauseButton.Width = 50;
                PauseButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");
                PausedText.Visibility = Visibility.Collapsed;
                simPaused = false;
            }
            else
            {
                PauseButton.Content = "Unpause";
                PauseButton.Width = 60;
                PauseButton.SetResourceReference(BackgroundProperty, "SuccessGradient");
                PausedText.Visibility = Visibility.Visible;
                simPaused = true;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            PauseButton.Content = "Pause";
            PauseButton.Width = 50;
            PauseButton.SetResourceReference(BackgroundProperty, "RoundedTextBoxGrad");

            readyToStop = true;
            simPaused = false;

            AbleControlButton(false);
        }

        private void GUILess_Click(object sender, RoutedEventArgs e)
        {
            GUISize = Math.Max(0.2, GUISize - 0.1);
            GUIPercentLabel.Text = $"{(int)(GUISize * 100)}%";
            Rerender();
        }

        private void GUIMore_Click(object sender, RoutedEventArgs e)
        {
            GUISize = Math.Min(3, GUISize + 0.1);
            GUIPercentLabel.Text = $"{(int)(GUISize * 100)}%";
            Rerender();
        }

        private void UpdateColorsButton_Click(object sender, RoutedEventArgs e)
        {
            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;

            double sizeShiftX = width * CurrentSize / 2 - width / 2;
            double sizeShiftY = height * CurrentSize / 2 - height / 2;

            AllVehicleNodesObjectCreator.ClearAllColors();
            RenderVehicles(CurrentSize);
            ShiftOnlyVehicles(new NegSize(TotalShiftWidth - sizeShiftX, TotalShiftHeight - sizeShiftY));
        }

        private void SpeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SpeedTextBox.Text, out simulationSpeed))
            {
                SpeedTextBox.SetResourceReference(TextBox.BorderBrushProperty, "RoundedTextBoxGrad");
            }
            else
            {
                SpeedTextBox.SetResourceReference(TextBox.BorderBrushProperty, "ErrorGradient");
            }
        }

        private void CloseAllMenus()
        {
            GraphViewGrid.Visibility = Visibility.Hidden;
            GridControlButtons.Visibility = Visibility.Hidden;
            VehiclesTableGrid.Visibility = Visibility.Hidden;
            PointsTableGrid.Visibility = Visibility.Hidden;
            OutputGrid.Visibility = Visibility.Hidden;
            TasksTableGrid.Visibility = Visibility.Hidden;
        }

        private void GraphMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ViewLabel.Text = "Graph view";
            GraphViewGrid.Visibility = Visibility.Visible;
            GridControlButtons.Visibility = Visibility.Visible;
        }

        private void VehiclesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ViewLabel.Text = "Vehicles table";
            VehiclesTableGrid.Visibility = Visibility.Visible;
        }

        private void PointsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ViewLabel.Text = "Points table";
            PointsTableGrid.Visibility = Visibility.Visible;
        }

        private void OutputMenuButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            ViewLabel.Text = "Output";
            OutputGrid.Visibility = Visibility.Visible;
        }

        private void DistributedTasksButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllMenus();
            TasksDataGrid.ItemsSource = DistributedTasksShort;
            ViewLabel.Text = "Distributed tasks";
            TasksTableGrid.Visibility = Visibility.Visible;
        }
    }
}
