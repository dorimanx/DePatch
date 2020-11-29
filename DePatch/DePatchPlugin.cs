using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using HarmonyLib;
using NLog;
using Sandbox;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.Server.Managers;
using Torch.Session;
using VRage.Game;

namespace DePatch
{
    public class DePatchPlugin : TorchPluginBase, IWpfPlugin, ITorchPlugin, IDisposable
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static DePatchPlugin Instance;

        private TorchSessionManager _sessionManager;

        public Persistent<DeConfig> ConfigPersistent;

        public UserControlDepatch Control;

        public DeConfig Config => ConfigPersistent?.Data;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Instance = this;
            SetupConfig();

            if (Config.CheckForUpdates && DeUpdater.CheckAndUpdate(this, torch))
            {
                Process.Start(Assembly.GetAssembly(typeof(InstanceManager)).Location, string.Join(" ", Environment.GetCommandLineArgs()));
                Environment.Exit(0);
            }
            if (!Config.Enabled)
                return;

            new Harmony("net.ltp.depatch").PatchAll();

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();

            Config.Mods.ForEach(delegate (ulong m)
            {
                _sessionManager.AddOverrideMod(m);
            });
            if (Config.DamageThreading)
                _sessionManager.AddOverrideMod(2274830517UL);

            Log.Info("Mod Loader Complete overriding");
            if (_sessionManager != null)
                Torch.GameStateChanged += Torch_GameStateChanged;
        }

        private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
        {
            if (newState != TorchGameState.Loaded)
                return;

            if (Config.PveZoneEnabled)
                PVE.Init(this);

            DrillSettings.InitDefinitions();

            if (Instance.Config.ProtectGrid)
                MyGridDeformationPatch.Init();

            MySession.Static.OnSavingCheckpoint += Static_OnSavingCheckpoint;

            if (Config.DamageThreading)
                SessionPatch.Timer.Start();
        }

        private void Static_OnSavingCheckpoint(MyObjectBuilder_Checkpoint obj) => new Thread((ThreadStart)delegate
                                                                                {
                                                                                    lock (MyShipDrillParallelPatch.pendingDrillers)
                                                                                    {
                                                                                        Thread.Sleep(5000);
                                                                                    }
                                                                                }).Start();

        public void LoadConfig()
        {
            if (ConfigPersistent?.Data != null)
                ConfigPersistent = Persistent<DeConfig>.Load(Path.Combine(StoragePath, "DePatch.cfg"), true);
        }

        public void SetupConfig()
        {
            try
            {
                ConfigPersistent = Persistent<DeConfig>.Load(Path.Combine(base.StoragePath, "DePatch.cfg"));
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            if (ConfigPersistent?.Data != null)
                return;

            Log.Info("Create Default Config, because none was found!");
            ConfigPersistent = new Persistent<DeConfig>(Path.Combine(base.StoragePath, "DePatch.cfg"), new DeConfig());
            ConfigPersistent.Save(null);
        }

        public UserControl GetControl()
        {
            if (Control == null)
                Control = new UserControlDepatch(this);

            return Control;
        }
        public override void Dispose()
        {
            if (_sessionManager != null)
            {
                Torch.GameStateChanged -= Torch_GameStateChanged;
            }
            if (_sessionManager != null)
                _sessionManager = null;
        }
    }
}
