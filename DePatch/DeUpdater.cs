﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using Torch;
using Torch.API;
using Torch.Server.Managers;

namespace DePatch
{
	internal static class DeUpdater
	{
		// Token: 0x06000073 RID: 115 RVA: 0x0000352C File Offset: 0x0000172C
		public static bool CheckAndUpdate(TorchPluginBase plugin, ITorchBase torch)
		{
			DeUpdater.Log.Info("Checking for updates...");
			try
			{
				CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
				cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
				using (Stream responseStream = ((HttpWebResponse)((HttpWebRequest)WebRequest.Create("http://ltps.space/torchapi/DePatchVersion.info")).GetResponse()).GetResponseStream())
				{
					string text = responseStream.ReadString(Encoding.UTF8);
					if (float.Parse(text, NumberStyles.Any, cultureInfo) > float.Parse(plugin.Version, NumberStyles.Any, cultureInfo))
					{
						DeUpdater.Log.Info("New version available!");
						DeUpdater.Log.Warn("Downloading Update " + plugin.Version + " => " + text);
						string text2 = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(InstanceManager)).Location), "Plugins"), "DePatch.zip");
						DeUpdater.Log.Warn(text2);
						File.Delete(text2);
						using (WebClient webClient = new WebClient())
						{
							webClient.DownloadFile("http://ltps.space/torchapi/DePatch.zip", text2);
						}
						DeUpdater.Log.Info("Update Sucsesful");
						DeUpdater.Log.Warn("Force torch restarting...");
						return true;
					}
					DeUpdater.Log.Info("Up to date.");
				}
			}
			catch (Exception value)
			{
				DeUpdater.Log.Error<Exception>(value);
				DeUpdater.Log.Fatal("Error while updating");
			}
			return false;
		}

		// Token: 0x04000040 RID: 64
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	}
}
