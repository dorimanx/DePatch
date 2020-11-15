using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace DePatch
{
	public partial class UserControl2 : UserControl
	{
		public UserControl2()
		{
			this.InitializeComponent();
			this.DrillsGrid.ItemsSource = (IEnumerable) DrillSettings.drills;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			object selectedItem = this.DrillsGrid.SelectedItem;
      		if (selectedItem == null || !(selectedItem is DrillSettings))
        		return;

			DrillSettings.drills.Remove((DrillSettings)selectedItem);
		}
	}
}
