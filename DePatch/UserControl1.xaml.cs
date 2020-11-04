using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Torch.API;

namespace DePatch
{
	// Token: 0x02000028 RID: 40
	public partial class UserControl1 : UserControl
	{
		// Token: 0x060000B7 RID: 183 RVA: 0x0000512F File Offset: 0x0000332F
		public UserControl1()
		{
			this.InitializeComponent();
		}

		public UserControl1(DePatchPlugin plugin) : this()
		{
			this._instance = plugin;
			base.DataContext = plugin.Config;
			this.ModsBlock.Text = string.Join<ulong>(";", plugin.Config.Mods);
			this.RaycastLimitTextBox.Text = plugin.Config.RaycastLimit.ToString("0");
			this.TimerDelayTextBox.Text = plugin.Config.TimerMinDelay.ToString("0");
			if (plugin.Config.BeaconSubTypes.Count == 0)
			{
				plugin.Config.BeaconSubTypes.AddArray(new string[]
				{
					"LargeBlockBeacon",
					"SmallBlockBeacon"
				});
			}
			this.subtypesTextBlock.Text = string.Join(";", plugin.Config.BeaconSubTypes);
			this.RedTextBox.Text = this._instance.Config.RedAlertText;
			this.WOBTextBox.Text = this._instance.Config.WithOutBeaconText;
			this.WDGTextBox.Text = this._instance.Config.WithDefaultNameText;
			int count = this._instance.Config.ShipTools.Count;
			(from b in this._instance.Config.ShipTools
			select ShipToolDeserializer.Deserialize(b)).ForEach(delegate(ShipTool b)
			{
				ShipTool.shipTools.Add(b);
			});
			(from b in this._instance.Config.DrillsSettings
			select DrillSettings.Deserialize(b)).ForEach(delegate(DrillSettings b)
			{
				DrillSettings.drills.Add(b);
			});
			this.ShipToolsGrid.ItemsSource = ShipTool.shipTools;
			this.DrillModeCombobox.ItemsSource = Enum.GetValues(typeof(DrillingMode)).Cast<DrillingMode>();
			this.DrillModeCombobox.SelectedIndex = (int)this._instance.Config.ParallelDrill;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (ModsBlock.Text.Length > 10)
			{
				string[] array = this.ModsBlock.Text.Split(new char[]
				{
					';'
				});
				if (array.Length != 0)
				{
					this._instance.Config.Mods.Clear();
					foreach (string s in array)
					{
						ulong item;
						if (!ulong.TryParse(s, out item))
						{
							break;
						}
						this._instance.Config.Mods.Add(item);
						item = 0UL;
					}
				}
			}
			this.ModsBlock.Text = string.Join<ulong>(";", this._instance.Config.Mods);
			if (this.subtypesTextBlock.Text.Length > 1)
			{
				this._instance.Config.BeaconSubTypes.Clear();
				string[] itemsToAdd = this.subtypesTextBlock.Text.Split(new char[]
				{
					';'
				}, StringSplitOptions.RemoveEmptyEntries);
				this._instance.Config.BeaconSubTypes.AddArray(itemsToAdd);
			}
			this.subtypesTextBlock.Text = string.Join(";", this._instance.Config.BeaconSubTypes);
			this._instance.Config.RedAlertText = this.RedTextBox.Text;
			this._instance.Config.WithOutBeaconText = this.WOBTextBox.Text;
			this._instance.Config.WithDefaultNameText = this.WDGTextBox.Text;
			this._instance.Config.ShipTools = (from t in ShipTool.shipTools
			select ShipToolSerializer.Serialize(t)).ToList<string>();
			this._instance.Config.DrillsSettings = (from t in DrillSettings.drills
			select DrillSettings.Serialize(t)).ToList<string>();
			this._instance.Config.ParallelDrill = (DrillingMode)this.DrillModeCombobox.SelectedIndex;
			this._instance.ConfigPersistent.Save(null);
			this._instance.SetupConfig();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			ShipTool.shipTools.Add(new ShipTool
			{
				Speed = ShipTool.DEFAULT_SPEED,
				Subtype = "New",
				Type = ToolType.Welder
			});
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			object selectedItem = this.ShipToolsGrid.SelectedItem;
			if (selectedItem != null && selectedItem is ShipTool)
			{
				ShipTool.shipTools.Remove((ShipTool)selectedItem);
			}
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			if (this.IgnoreDrillSubtypes.IsChecked ?? true)
			{
				MessageBox.Show("Check Ignore Subtypes Disabled!");
				return;
			}
			if (this._instance.Torch.GameState != TorchGameState.Loaded)
			{
				MessageBox.Show("Start Game Before Editing!");
				return;
			}
			new Window
			{
				Title = "Subtypes Editor",
				Content = new UserControl2(),
				Height = 520.0,
				Width = 370.0
			}.ShowDialog();
		}

		private DePatchPlugin _instance;
	}
}
