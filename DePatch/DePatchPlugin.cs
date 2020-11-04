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
using Torch.API.Plugins;
using Torch.Server.Managers;
using Torch.Session;
using VRage.Game;

namespace DePatch
{
	public class DePatchPlugin : TorchPluginBase, IWpfPlugin, ITorchPlugin, IDisposable
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600006B RID: 107 RVA: 0x000032E9 File Offset: 0x000014E9
		public DeConfig Config
		{
			get
			{
				Persistent<DeConfig> configPersistent = this.ConfigPersistent;
				if (configPersistent == null)
				{
					return null;
				}
				return configPersistent.Data;
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000032FC File Offset: 0x000014FC
		public override void Init(ITorchBase torch)
		{
			base.Init(torch);
			DePatchPlugin.Instance = this;
			this.SetupConfig();
			if (this.Config.CheckForUpdates && DeUpdater.CheckAndUpdate(this, torch))
			{
				Process.Start(Assembly.GetAssembly(typeof(InstanceManager)).Location, string.Join(" ", Environment.GetCommandLineArgs()));
				Environment.Exit(0);
			}
			if (!this.Config.Enabled)
			{
				return;
			}
			new Harmony("net.ltp.depatch").PatchAll();
			TorchSessionManager manager = torch.GetManager<TorchSessionManager>();
			this.Config.Mods.ForEach(delegate(ulong m)
			{
				manager.AddOverrideMod(m);
			});
			if (this.Config.DamageThreading)
			{
				manager.AddOverrideMod((ulong)2274830517);
			}
			DePatchPlugin.Log.Info("Mod Loader Complete overriding");
			torch.GameStateChanged += this.Torch_GameStateChanged;
		}

		private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
		{
			if (newState == TorchGameState.Loaded)
			{
				if (this.Config.PveZoneEnabled)
				{
					PVE.Init(this);
				}
				DrillSettings.InitDefinitions();
				MySession.Static.OnSavingCheckpoint += this.Static_OnSavingCheckpoint;
				if (this.Config.DamageThreading)
				{
					SessionPatch.Timer.Start();
				}
			}
		}

		private void Static_OnSavingCheckpoint(MyObjectBuilder_Checkpoint obj)
		{
			new Thread(delegate()
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
			string path = Path.Combine(base.StoragePath, "DePatch.cfg");
			try
			{
				this.ConfigPersistent = Persistent<DeConfig>.Load(path, true);
			}
			catch (Exception value)
			{
				DePatchPlugin.Log.Warn<Exception>(value);
			}
			Persistent<DeConfig> configPersistent = this.ConfigPersistent;
			if (((configPersistent != null) ? configPersistent.Data : null) == null)
			{
				DePatchPlugin.Log.Info("Create Default Config, because none was found!");
				this.ConfigPersistent = new Persistent<DeConfig>(path, new DeConfig());
				this.ConfigPersistent.Save(null);
			}
		}

		public UserControl GetControl()
		{
			if (this.Control == null)
			{
				this.Control = new UserControl1(this);
			}
			return this.Control;
		}

		// Token: 0x0400003C RID: 60
		public static readonly Logger Log = LogManager.GetCurrentClassLogger();

		// Token: 0x0400003D RID: 61
		public static DePatchPlugin Instance;

		// Token: 0x0400003E RID: 62
		public Persistent<DeConfig> ConfigPersistent;

		// Token: 0x0400003F RID: 63
		public UserControl1 Control;
	}
}
