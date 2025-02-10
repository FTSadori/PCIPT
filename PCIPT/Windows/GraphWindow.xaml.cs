using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
    /// <summary>
    /// Логика взаимодействия для GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window
    {
        public GraphWindow()
        {
            InitializeComponent();

            Nodes = CsvHandler.GetAllFromFile<NodeDto>("Nodes.csv");
            Routes = CsvHandler.GetAllFromFile<RouteDto>("Routes.csv");
        }

        public static void DoCmd(ThreadStart th)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, th);
        }

        private record NegSize(double Width, double Height);

        private Dictionary<int, NegSize> nodesCoords = new();
        private Dictionary<int, NegSize> nodesRealSize = new();

        private readonly List<NodeDto> Nodes;
        private readonly List<RouteDto> Routes;

        private double CurrentSize = 1;
        private bool Init = false;
        private double TotalShiftHeight = 0;
        private double TotalShiftWidth = 0;

        private void InitCircleGraph(List<NodeDto> nodes)
        {
            nodesCoords.Clear();

            int N = nodes.Count;

            double angle = 2.0 * Math.PI / N;

            for (int i = 0; i < N; i++)
            {
                nodesCoords.Add(nodes[i].Id, new NegSize(Math.Cos(i * angle), Math.Sin(i * angle)));
            }

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
        }

        private void AddToTotalOffset(NegSize Offset)
        {
            TotalShiftHeight += Offset.Height;
            TotalShiftWidth += Offset.Width;
        }

        private void AutoNormalizeElementsOnGraph(double Size)
        {
            int i = 0;
            foreach (var child in GraphCanvas.Children)
            {
                if (child is Grid grid)
                {
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) - grid.ActualWidth / 2);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) - grid.ActualHeight / 2);
                    nodesRealSize.Add(i++, new NegSize(grid.ActualWidth / 2 / Size, grid.ActualHeight / 2 / Size));
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
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) - nodesRealSize[i].Width * Size);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) - nodesRealSize[i].Height * Size);
                    grid.Visibility = Visibility.Visible;
                    ++i;
                }
            }
        }

        private void RenderCircleGraph(List<NodeDto> nodes, List<RouteDto> routes, double size, NegSize shift)
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

                GraphCanvas.Children.Add(CreateLine(realPoint1, realPoint2, size));
            }

            foreach (var node in nodes) 
            {
                NegSize realPoint = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[node.Id].Width,
                    zeroPoint.Height + NormalDistance * nodesCoords[node.Id].Height);

                var nodeObj = CreateNodeGrid(node.Name, node.Id, realPoint, size);
                nodeObj.Visibility = Visibility.Hidden;
                GraphCanvas.Children.Add(nodeObj);
            }

            ShiftGraph(shift);

            if (nodesRealSize.Count == 0)
            {
                renderThread = new(delegate ()
                {
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
            stack.Background = Brushes.LightGray;
            stack.Margin = new Thickness(3 * Size);

            stack.Children.Add(new TextBlock()
            {
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(5 * Size),
                Margin = new Thickness(0, 0, 0, 1 * Size),
                FontSize = 16 * Size,
                Background = Brushes.White,
                Text = Name,
            });

            stack.Children.Add(new TextBlock()
            {
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3 * Size),
                FontSize = 12 * Size,
                Background = Brushes.White,
                Text = "ID: " + Id.ToString(),
            });

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
                RenderCircleGraph(Nodes, Routes, CurrentSize, new NegSize(TotalShiftWidth, TotalShiftHeight));
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
            AddToTotalOffset(new(0, +20));
            ShiftGraph(new(0, +20));
        }

        private void SizePlus_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize += 0.1;
            Rerender();
        }

        private void SizeMinus_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize = Math.Max(CurrentSize - 0.1, 0.1);
            Rerender();
        }

        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            CurrentSize = 1;
            TotalShiftHeight = 0;
            TotalShiftWidth = 0;
            Rerender();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.I:
                    InitCircleGraph(Nodes);
                    RenderCircleGraph(Nodes, Routes, CurrentSize, new NegSize(TotalShiftWidth, TotalShiftHeight));
                    break;
                case Key.Y:
                    RenderCircleGraph(Nodes, Routes, CurrentSize, new NegSize(TotalShiftWidth, TotalShiftHeight));
                    break;
                case Key.X:
                    AutoNormalizeElementsOnGraph(CurrentSize);
                    break;
                case Key.Down:
                    AddToTotalOffset(new(0, +10));
                    ShiftGraph(new(0, +10));
                    break;
                case Key.Up:
                    AddToTotalOffset(new(0, -10));
                    ShiftGraph(new(0, -10));
                    break;
                case Key.Left:
                    AddToTotalOffset(new(+10, 0));
                    ShiftGraph(new(+10, 0));
                    break;
                case Key.Right:
                    AddToTotalOffset(new(-10, 0));
                    ShiftGraph(new(-10, 0));
                    break;
            }
        }
    }
}
