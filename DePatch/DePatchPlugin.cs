using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using DePatch.GamePatches;
using DePatch.KEEN_BUG_FIXES;
using DePatch.PVEZONE;
using DePatch.ShipTools;
using DePatch.VoxelProtection;
using HarmonyLib;
using NLog;
using Sandbox;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.Managers.PatchManager;
using Torch.Server.Managers;
using Torch.Session;

namespace DePatch
{
    public class DePatchPlugin : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static DePatchPlugin Instance;

        private readonly Harmony _harmony = new Harmony("Dori.DePatchPlugin");

        private TorchSessionManager _sessionManager;

        private Persistent<DeConfig> _configPersistent;

        public UserControlDepatch Control;

        public static bool GameIsReady;

        public static int StaticTick = 0;

        public DeConfig Config => _configPersistent?.Data;

        public void Save() => _configPersistent.Save();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Instance = this;
            SetupConfig();

            /*
            if (Config.CheckForUpdates && DeUpdater.CheckAndUpdate(this, torch))
            {
                Process.Start(Assembly.GetAssembly(typeof(InstanceManager)).Location, string.Join(" ", Environment.GetCommandLineArgs()));
                Environment.Exit(0);
            }
            */

            if (!Config.Enabled)
                return;

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();

            Config.Mods.ForEach(delegate (ulong m)
            {
                _sessionManager.AddOverrideMod(m);
            });

            Log.Info("Mod Loader Complete overriding");
            if (_sessionManager != null)
                Torch.GameStateChanged += Torch_GameStateChanged;
        }

        private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
        {
            if (!Config.Enabled)
                return;

            var patchManager = Torch.Managers.GetManager<PatchManager>();
            var context = patchManager.AcquireContext();

            if (newState == TorchGameState.Loading)
            {
                _harmony.PatchAll();

                if (Config.UpdateAfterSimulation10FIX
                    || Config.UpdateAfterSimulation100FIX
                    || Config.UpdateBeforeSimulation100FIX
                    || Config.ParallelUpdateHandlerAfterSimulationFIX)
                {
                    KEEN_UpdateSimulationFixes.Patch(context);
                }

                if (Config.TurretsAimFix)
                    KEEN_MyTurretTargetFlagsFix.PatchMyTurretTarget();

                if (Config.UpdateComponentsFix)
                    KEEN_UpdateComponentsFix.Patch(context);

                if (Config.UpdateOnceBeforeFrameFix)
                    KEEN_UpdateOnceBeforeFrameFix.Patch(context);
            }

            if (newState != TorchGameState.Loaded)
                return;

            GameIsReady = true;

            if (Config.PveZoneEnabled)
            {
                PVE.Init(this);
                FreezerPatch.ApplyPatch(_harmony, Torch);
            }

            DrillSettings.InitDefinitions();

            if (Config.ProtectGrid)
                MyGridDeformationPatch.Init();

            patchManager.Commit();
        }

        public override void Update()
        {
            if (!MySession.Static.IsSaveInProgress)
                MyGasTankPatch.UpdateTanks();

            ServerAliveLog.UpdateLOG();
            MyPVESafeZoneAction.UpdateBoot();
        }

        public void LoadConfig()
        {
            if (_configPersistent?.Data != null)
                _configPersistent = Persistent<DeConfig>.Load(Path.Combine(StoragePath, "DePatch.cfg"));
        }

        private void SetupConfig()
        {
            try
            {
                _configPersistent = Persistent<DeConfig>.Load(Path.Combine(StoragePath, "DePatch.cfg"));
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            if (_configPersistent?.Data != null)
                return;

            Log.Info("Create Default Config, because none was found!");
            _configPersistent = new Persistent<DeConfig>(Path.Combine(StoragePath, "DePatch.cfg"), new DeConfig());
            _configPersistent.Save(null);
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
            _sessionManager = null;
        }
    }
}
