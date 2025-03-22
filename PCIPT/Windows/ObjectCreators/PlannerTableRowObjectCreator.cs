using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PCIPT.Windows.ObjectCreators
{
    public sealed class PlannerTableRowObjectCreator
    {
        public delegate void OpenTableDelegate(int tableIndex);

        public static StackPanel GetObject(string text, int tableIndex, OpenTableDelegate openTableDelegate)
        {
            StackPanel stackPanel = new()
            {
                Orientation = Orientation.Horizontal,
                Width = 154f,
            };

            TextBlock textBlock = new()
            {
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = 124f,
                TextAlignment = TextAlignment.Left,
                Text = text,
            };
            textBlock.SetResourceReference(TextBlock.StyleProperty, "SmallPrettyText");

            Button button = new()
            {
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Height = 20,
                Width = 20,
                Content = ">",
            };
            button.Click += delegate (object sender, RoutedEventArgs e) { openTableDelegate(tableIndex); };
            button.SetResourceReference(Button.StyleProperty, "RegButton");
            button.SetResourceReference(Button.BackgroundProperty, "RoundedTextBoxGrad");

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);

            return stackPanel;
        }
    }
}
