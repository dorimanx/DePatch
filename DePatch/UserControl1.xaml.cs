using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Torch.API;
using Torch.Views;

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
            SlowSimulationUpdate1.Text = plugin.Config.SlowPBUpdate1.ToString("0");
            SlowSimulationUpdate10.Text = plugin.Config.SlowPBUpdate10.ToString("0");
            SlowSimulationUpdate100.Text = plugin.Config.SlowPBUpdate100.ToString("0");


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
            if (IgnorePBSubTypesHere.Text.Length > 1)
            {
                MyProgramBlockSlow.Init();
            }
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

            if (IgnorePBSubTypesHere.Text.Length > 1)
            {
                MyProgramBlockSlow.Init();
            }

            if (ModsBlock.Text.Length == 0)
            {
                Plugin.Config.Mods.Clear();
            }
            else
            {
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
            }

            Plugin.Save();
            Plugin.LoadConfig();
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
            object selectedItem = ShipToolsGrid.SelectedItem;
            if (selectedItem != null && selectedItem is ShipTool)
            {
                ShipTool.shipTools.Remove((ShipTool)selectedItem);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (IgnoreDrillSubtypes.IsChecked ?? true)
            {
                MessageBox.Show("Check Ignore Subtypes Disabled!");
                return;
            }
            if (Plugin.Torch.GameState != TorchGameState.Loaded)
            {
                MessageBox.Show("Start Game Before Editing!");
                return;
            }
            new Window
            {
                Title = "Subtypes Editor",
                Content = new UserControl2(),
                Height = 515.0,
                Width = 515.0
            }.ShowDialog();
        }

        private void EditBlocks_OnClick(object sender, RoutedEventArgs e)
        {
            new CollectionEditor
            {
                Owner = Window.GetWindow(this)
            }.Edit((ICollection<string>)Plugin.Config.TargetedBlocks, "Disabled Blocks - Use typeId and/or subtypeId");
        }

        private void EditFactions_OnClick(object sender, RoutedEventArgs e)
        {
            new CollectionEditor
            {
                Owner = Window.GetWindow(this)
            }.Edit((ICollection<string>)Plugin.Config.ExemptedFactions, "Exempted Factions/Players - Use player names or faction tags");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
            Plugin.LoadConfig();
        }
    }
}
