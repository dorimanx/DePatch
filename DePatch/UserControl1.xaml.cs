using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Torch.API;

namespace DePatch
{
    public partial class UserControlDepatch : UserControl
    {

        private DePatchPlugin Plugin { get; }

        public UserControlDepatch()
        {
            InitializeComponent();
        }

        public UserControlDepatch(DePatchPlugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
            ModsBlock.Text = string.Join(";", plugin.Config.Mods);
            RaycastLimitTextBox.Text = plugin.Config.RaycastLimit.ToString("0");
            TimerDelayTextBox.Text = plugin.Config.TimerMinDelay.ToString("0");
            if (plugin.Config.BeaconSubTypes.Count == 0)
            {
                plugin.Config.BeaconSubTypes.AddArray(new string[2]
                {
                    "LargeBlockBeacon",
                    "SmallBlockBeacon"
                });
            }
            subtypesTextBlock.Text = string.Join(";", plugin.Config.BeaconSubTypes);
            RedTextBox.Text = Plugin.Config.RedAlertText;
            WOBTextBox.Text = Plugin.Config.WithOutBeaconText;
            WDGTextBox.Text = Plugin.Config.WithDefaultNameText;
            int count = Plugin.Config.ShipTools.Count;
            Plugin.Config.ShipTools.Select(b => ShipToolDeserializer.Deserialize(b)).ForEach(b => ShipTool.shipTools.Add(b));
            Plugin.Config.DrillsSettings.Select(b => DrillSettings.Deserialize(b)).ForEach(b => DrillSettings.drills.Add(b));
            ShipToolsGrid.ItemsSource = ShipTool.shipTools;
            DrillModeCombobox.ItemsSource = Enum.GetValues(typeof(DrillingMode)).Cast<DrillingMode>();
            DrillModeCombobox.SelectedIndex = (int)Plugin.Config.ParallelDrill;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Plugin.LoadConfig();

            if (subtypesTextBlock.Text.Length > 1)
            {
                Plugin.Config.BeaconSubTypes.Clear();
                Plugin.Config.BeaconSubTypes.AddArray(subtypesTextBlock.Text.Split(new char[1]
                {
                    ';'
                }, StringSplitOptions.RemoveEmptyEntries));
            }
            subtypesTextBlock.Text = string.Join(";", Plugin.Config.BeaconSubTypes);
            Plugin.Config.RedAlertText = RedTextBox.Text;
            Plugin.Config.WithOutBeaconText = WOBTextBox.Text;
            Plugin.Config.WithDefaultNameText = WDGTextBox.Text;
            Plugin.Config.ShipTools = ShipTool.shipTools.Select(t => ShipToolSerializer.Serialize(t)).ToList();
            Plugin.Config.DrillsSettings = DrillSettings.drills.Select(t => DrillSettings.Serialize(t)).ToList();
            Plugin.Config.ParallelDrill = (DrillingMode)DrillModeCombobox.SelectedIndex;

            if (ModsBlock.Text.Length > 10 && ModsBlock.Text.Split(new char[]
                {
                    ';'
                }).Length != 0)
            {
                Plugin.Config.Mods.Clear();
                foreach (string s in ModsBlock.Text.Split(new char[]
            {
                    ';'
            }))
                {
                    if (!ulong.TryParse(s, out ulong item))
                    {
                        break;
                    }
                    Plugin.Config.Mods.Add(item);
                    item = 0UL;
                }
            }
            ModsBlock.Text = string.Join(";", Plugin.Config.Mods);

            Plugin.ConfigPersistent.Save(null);
            Plugin.SetupConfig();
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
            if (ShipToolsGrid.SelectedItem == null || !(ShipToolsGrid.SelectedItem is ShipTool))
                return;

            ShipTool.shipTools.Remove((ShipTool)ShipToolsGrid.SelectedItem);
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if ((IgnoreDrillSubtypes.IsChecked.HasValue ? (IgnoreDrillSubtypes.IsChecked.GetValueOrDefault() ? 1 : 0) : 1) != 0)
            {
                _ = (int)MessageBox.Show("Check Ignore Subtypes Disabled!");
            }
            else if (Plugin.Torch.GameState != TorchGameState.Loaded)
            {
                _ = (int)MessageBox.Show("Start Game Before Editing!");
            }
            else
            {
                Window window = new Window
                {
                    Title = "Subtypes Editor",
                    Content = new UserControl2(),
                    Height = 520.0,
                    Width = 370.0
                };
                window.ShowDialog();
            }
        }

        private void ModsBlock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
