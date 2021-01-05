using System.Windows;
using System.Windows.Controls;
using DePatch.ShipTools;

namespace DePatch
{
    public partial class UserControl2 : UserControl
    {
        public UserControl2()
        {
            InitializeComponent();
            DrillsGrid.ItemsSource = DrillSettings.drills;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DrillsGrid.SelectedItem;
            if (selectedItem != null && selectedItem is DrillSettings)
            {
                DrillSettings.drills.Remove((DrillSettings)selectedItem);
            }
        }
    }
}
