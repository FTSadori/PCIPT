using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PCIPT.Windows.ObjectCreators
{
    public sealed class ManageAccountsRowCreator
    {
        public delegate void ManageUserDelegate(string name);

        public static Border GetObject(int number, string name, string role, ManageUserDelegate manageUserDelegate, string currentUsersRole)
        {
            var border = new Border()
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            border.SetResourceReference(Border.BorderBrushProperty, "RoundedTextBoxGrad");

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };

            var innerBorder1 = new Border()
            {
                BorderThickness = new Thickness(0, 0, 1, 0),
                Width = 30,
            };
            innerBorder1.SetResourceReference(Border.BorderBrushProperty, "RoundedTextBoxGrad");
            var innerTextBlock1 = new TextBlock()
            {
                Text = number.ToString(),
                Margin = new Thickness(5, 0, 0, 0),
                TextAlignment = TextAlignment.Left,
                FontSize = 14,
            };
            innerTextBlock1.SetResourceReference(TextBlock.StyleProperty, "SmallPrettyText");
            innerBorder1.Child = innerTextBlock1;

            var innerBorder2 = new Border()
            {
                BorderThickness = new Thickness(0, 0, 1, 0),
                Width = 230,
            };
            innerBorder2.SetResourceReference(Border.BorderBrushProperty, "RoundedTextBoxGrad");
            var innerTextBlock2 = new TextBlock()
            {
                Text = name,
                Margin = new Thickness(5, 0, 0, 0),
                TextAlignment = TextAlignment.Left,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 14,
            };
            innerTextBlock2.SetResourceReference(TextBlock.StyleProperty, "SmallPrettyText");
            innerBorder2.Child = innerTextBlock2;

            var innerBorder3 = new Border()
            {
                BorderThickness = new Thickness(0, 0, 1, 0),
                Width = 120,
            };
            innerBorder3.SetResourceReference(Border.BorderBrushProperty, "RoundedTextBoxGrad");
            var innerTextBlock3 = new TextBlock()
            {
                Text = role,
                Margin = new Thickness(5, 0, 0, 0),
                TextAlignment = TextAlignment.Left,
                FontSize = 14,
            };
            innerTextBlock3.SetResourceReference(TextBlock.StyleProperty, "SmallPrettyText");
            innerBorder3.Child = innerTextBlock3;

            var innerBorder4 = new Border()
            {
                BorderThickness = new Thickness(0, 0, 0, 0),
                Width = 100,
            };
            innerBorder4.SetResourceReference(Border.BorderBrushProperty, "RoundedTextBoxGrad");
            var innerButton = new Button()
            {
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Height = 20,
                Width = 80,
                Content = "Manage",
            };
            innerButton.SetResourceReference(Button.BackgroundProperty, "RoundedTextBoxGrad");
            innerButton.SetResourceReference(Button.StyleProperty, "RegButton");
            innerButton.Click += delegate(object sender, RoutedEventArgs e) { manageUserDelegate(name); };
            if (role == "admin" || currentUsersRole != "admin")
            {
                innerButton.IsEnabled = false;
                innerButton.SetResourceReference(Button.BackgroundProperty, "DisableGradient");
            }
            innerBorder4.Child = innerButton;

            stackPanel.Children.Add(innerBorder1);
            stackPanel.Children.Add(innerBorder2);
            stackPanel.Children.Add(innerBorder3);
            stackPanel.Children.Add(innerBorder4);

            border.Child = stackPanel;

            return border;
        }
    }
}
