using System.Windows;
using System.Windows.Controls;

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
            if (DrillsGrid.SelectedItem == null || !(DrillsGrid.SelectedItem is DrillSettings))
            {
                return;
            }

            DrillSettings.drills.Remove((DrillSettings)DrillsGrid.SelectedItem);
        }
    }
}
