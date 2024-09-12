using PCIPT.Core.DataHandler;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        private record NegSize(double Width, double Height);

        private Dictionary<int, NegSize> nodesCoords = new();

        private List<NodeDto> Nodes;
        private List<RouteDto> Routes;

        private void InitCircleGraph(List<NodeDto> nodes)
        {
            nodesCoords.Clear();

            int N = nodes.Count;

            double angle = 2.0 * Math.PI / N;

            for (int i = 0; i < N; i++)
            {
                nodesCoords.Add(nodes[i].Id, new NegSize(Math.Cos(i * angle), Math.Sin(i * angle)));
            }
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

        private void NormalizeElementsOnGraph()
        {
            foreach (var child in GraphCanvas.Children)
            {
                if (child is Grid grid)
                {
                    Canvas.SetLeft(grid, Canvas.GetLeft(grid) - grid.ActualWidth / 2);
                    Canvas.SetBottom(grid, Canvas.GetBottom(grid) - grid.ActualHeight / 2);
                }
            }
        }

        private void RenderCircleGraph(List<NodeDto> nodes, List<RouteDto> routes)
        {
            GraphCanvas.Children.Clear();

            double NormalDistance = (Math.Min(GraphCanvas.ActualWidth, GraphCanvas.ActualHeight) / 2.0) * 0.8;

            NegSize zeroPoint = new(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2);
            
            foreach (var route in routes)
            {
                NegSize realPoint1 = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[route.SourceId].Width,
                    GraphCanvas.ActualHeight - zeroPoint.Height - NormalDistance * nodesCoords[route.SourceId].Height);

                NegSize realPoint2 = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[route.DestinationId].Width,
                    GraphCanvas.ActualHeight - zeroPoint.Height - NormalDistance * nodesCoords[route.DestinationId].Height);

                GraphCanvas.Children.Add(CreateLine(realPoint1, realPoint2));
            }

            foreach (var node in nodes) 
            {
                NegSize realPoint = new(
                    zeroPoint.Width + NormalDistance * nodesCoords[node.Id].Width,
                    zeroPoint.Height + NormalDistance * nodesCoords[node.Id].Height);
                GraphCanvas.Children.Add(CreateNodeGrid(node.Name, node.Id, realPoint));
            }
        }

        private void RenderNode()
        {
            Random random = new();
            GraphCanvas.Children.Add(CreateNodeGrid(new string('3', random.Next(10, 20)), random.Next(0, 100), new NegSize(random.NextDouble() * 300, random.NextDouble() * 350)));
        }

        private Line CreateLine(NegSize From, NegSize To)
        {
            Line line = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 3.0,

                X1 = From.Width,
                X2 = To.Width,

                Y1 = From.Height,
                Y2 = To.Height,
            };

            return line;
        }

        private Grid CreateNodeGrid(string Name, int Id, NegSize Coord)
        {
            Grid node = new()
            {
                Background = Brushes.Black,
            };

            StackPanel stack = new();
            stack.Background = Brushes.LightGray;
            stack.Margin = new Thickness(3);

            stack.Children.Add(new TextBlock()
            {
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 0, 1),
                FontSize = 16,
                Background = Brushes.White,
                Text = Name,
            });
            stack.Children.Add(new TextBlock()
            {
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3),
                FontSize = 12,
                Background = Brushes.White,
                Text = "ID: " + Id.ToString(),
            });

            node.Children.Add(stack);

            Canvas.SetLeft(node, Coord.Width);
            Canvas.SetBottom(node, Coord.Height);

            return node;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Q:
                    RenderNode();
                    break;
                case Key.I:
                    InitCircleGraph(Nodes);
                    break;
                case Key.Y:
                    RenderCircleGraph(Nodes, Routes);
                    break;
                case Key.X:
                    NormalizeElementsOnGraph();
                    break;

                case Key.Down:
                    ShiftGraph(new(0, -10));
                    break;
                case Key.Up:
                    ShiftGraph(new(0, +10));
                    break;
                case Key.Left:
                    ShiftGraph(new(-10, 0));
                    break;
                case Key.Right:
                    ShiftGraph(new(10, 0));
                    break;
            }
        }
    }
}
