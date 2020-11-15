using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Torch.API;

namespace DePatch
{
	public partial class UserControl1 : UserControl
	{

    	private DePatchPlugin _instance;

		public UserControl1()
		{
			this.InitializeComponent();
		}

		public UserControl1(DePatchPlugin plugin) : this()
		{
			this._instance = plugin;
			this.DataContext = (object) plugin.Config;
			this.ModsBlock.Text = string.Join<ulong>(";", plugin.Config.Mods);
			this.RaycastLimitTextBox.Text = plugin.Config.RaycastLimit.ToString("0");
			this.TimerDelayTextBox.Text = plugin.Config.TimerMinDelay.ToString("0");
			if (plugin.Config.BeaconSubTypes.Count == 0)
			{
				plugin.Config.BeaconSubTypes.AddArray<string>(new string[2]
				{
					"LargeBlockBeacon",
					"SmallBlockBeacon"
				});
			}
			this.subtypesTextBlock.Text = string.Join(";", (IEnumerable<string>) plugin.Config.BeaconSubTypes);
			this.RedTextBox.Text = this._instance.Config.RedAlertText;
			this.WOBTextBox.Text = this._instance.Config.WithOutBeaconText;
			this.WDGTextBox.Text = this._instance.Config.WithDefaultNameText;
			int count = this._instance.Config.ShipTools.Count;
      		this._instance.Config.ShipTools.Select<string, ShipTool>((Func<string, ShipTool>) (b => ShipToolDeserializer.Deserialize(b))).ForEach<ShipTool>((Action<ShipTool>) (b => ShipTool.shipTools.Add(b)));
      		this._instance.Config.DrillsSettings.Select<string, DrillSettings>((Func<string, DrillSettings>) (b => DrillSettings.Deserialize(b))).ForEach<DrillSettings>((Action<DrillSettings>) (b => DrillSettings.drills.Add(b)));
			this.ShipToolsGrid.ItemsSource = (IEnumerable) ShipTool.shipTools;
			this.DrillModeCombobox.ItemsSource = (IEnumerable) Enum.GetValues(typeof(DrillingMode)).Cast<DrillingMode>();
			this.DrillModeCombobox.SelectedIndex = (int)this._instance.Config.ParallelDrill;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (this.subtypesTextBlock.Text.Length > 1)
			{
				this._instance.Config.BeaconSubTypes.Clear();
        		this._instance.Config.BeaconSubTypes.AddArray<string>(this.subtypesTextBlock.Text.Split(new char[1]
				{
					';'
				}, StringSplitOptions.RemoveEmptyEntries));
			}
      		this.subtypesTextBlock.Text = string.Join(";", (IEnumerable<string>) this._instance.Config.BeaconSubTypes);
			this._instance.Config.RedAlertText = this.RedTextBox.Text;
			this._instance.Config.WithOutBeaconText = this.WOBTextBox.Text;
			this._instance.Config.WithDefaultNameText = this.WDGTextBox.Text;
			this._instance.Config.ShipTools = ShipTool.shipTools.Select<ShipTool, string>((Func<ShipTool, string>) (t => ShipToolSerializer.Serialize(t))).ToList<string>();
      		this._instance.Config.DrillsSettings = DrillSettings.drills.Select<DrillSettings, string>((Func<DrillSettings, string>) (t => DrillSettings.Serialize(t))).ToList<string>();
			this._instance.Config.ParallelDrill = (DrillingMode)this.DrillModeCombobox.SelectedIndex;

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

			this._instance.ConfigPersistent.Save((string) null);
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
     		 if (selectedItem == null || !(selectedItem is ShipTool))
        		return;

			ShipTool.shipTools.Remove((ShipTool)selectedItem);
		}


		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
      		bool? isChecked = this.IgnoreDrillSubtypes.IsChecked;
      		if ((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 1) != 0)
      		{
        		int num1 = (int) MessageBox.Show("Check Ignore Subtypes Disabled!");
			}
      		else if (this._instance.Torch.GameState != TorchGameState.Loaded)
			{
        		int num2 = (int) MessageBox.Show("Start Game Before Editing!");
      		}
      		else
      		{
        		Window window = new Window();
        		window.Title = "Subtypes Editor";
        		window.Content = (object) new UserControl2();
        		window.Height = 520.0;
        		window.Width = 370.0;
        		window.ShowDialog();
      		}
    	}
	}
}
