using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls;
using HarmonyLib;
using NLog;
using Sandbox;
using Sandbox.Game.Weapons;
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

		public UserControl1 Control;

		public DeConfig Config
		{
			get
			{
        		return this.ConfigPersistent?.Data;
			}
		}

		public override void Init(ITorchBase torch)
		{
			base.Init(torch);
			DePatchPlugin.Instance = this;
			this.SetupConfig();
			if (this.Config.CheckForUpdates && DeUpdater.CheckAndUpdate((TorchPluginBase)this, torch))
			{
				Process.Start(Assembly.GetAssembly(typeof(InstanceManager)).Location, string.Join(" ", Environment.GetCommandLineArgs()));
				Environment.Exit(0);
			}
			if (!this.Config.Enabled)
				return;

			new Harmony("net.ltp.depatch").PatchAll();

			this._sessionManager = base.Torch.Managers.GetManager<TorchSessionManager>();

			this.Config.Mods.ForEach(delegate (ulong m)
			{
				this._sessionManager.AddOverrideMod(m);
			});
			if (this.Config.DamageThreading)
				this._sessionManager.AddOverrideMod(2274830517UL);

			DePatchPlugin.Log.Info("Mod Loader Complete overriding");
			if (this._sessionManager != null)
				Torch.GameStateChanged += this.Torch_GameStateChanged;
		}

		private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
        {
            if (newState != TorchGameState.Loaded)
                return;

            if (this.Config.PveZoneEnabled)
                PVE.Init(this);

            DrillSettings.InitDefinitions();

            if (DePatchPlugin.Instance.Config.ProtectGrid)
                MyGridDeformationPatch.Init();

            MySession.Static.OnSavingCheckpoint += new Action<MyObjectBuilder_Checkpoint>(this.Static_OnSavingCheckpoint);

            if (this.Config.DamageThreading)
                SessionPatch.Timer.Start();
        }

        private void Static_OnSavingCheckpoint(MyObjectBuilder_Checkpoint obj)
        {
            new Thread(delegate ()
              {
                  List<MyShipDrill> pendingDrillers = MyShipDrillParallelPatch.pendingDrillers;
                  lock (pendingDrillers)
                  {
                      Thread.Sleep(5000);
                  }
              }).Start();
        }

        public void SetupConfig()
		{
			string path = Path.Combine(this.StoragePath, "DePatch.cfg");
			try
			{
				this.ConfigPersistent = Persistent<DeConfig>.Load(path, true);
			}
			catch (Exception ex)
			{
				DePatchPlugin.Log.Warn<Exception>(ex);
			}
     		if (this.ConfigPersistent?.Data != null)
        		return;

			DePatchPlugin.Log.Info("Create Default Config, because none was found!");
			this.ConfigPersistent = new Persistent<DeConfig>(path, new DeConfig());
      		this.ConfigPersistent.Save((string) null);
		}

		public UserControl GetControl()
		{
			if (this.Control == null)
				this.Control = new UserControl1(this);

      		return (UserControl) this.Control;
		}
		public override void Dispose()
		{
			if (this._sessionManager != null)
			{
				Torch.GameStateChanged -= this.Torch_GameStateChanged;
			}
			if (this._sessionManager != null)
                this._sessionManager = null;
        }
	}
}
