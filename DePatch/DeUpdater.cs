using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using NLog;
using Torch;
using Torch.API;
using Torch.Server.Managers;

namespace DePatch
{
    internal static class DeUpdater
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static bool CheckAndUpdate(TorchPluginBase plugin, ITorchBase torch)
        {
            Log.Info("Checking for updates...");
            try
            {
                CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
                using (Stream responseStream = ((HttpWebResponse)((HttpWebRequest)WebRequest.Create("http://google.com")).GetResponse()).GetResponseStream())
                {
                    string text = responseStream.ReadString(Encoding.UTF8);
                    if (float.Parse(text, NumberStyles.Any, cultureInfo) > float.Parse(plugin.Version, NumberStyles.Any, cultureInfo))
                    {
                        Log.Info("New version available!");
                        Log.Warn("Downloading Update " + plugin.Version + " => " + text);
                        string text2 = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(InstanceManager)).Location), "Plugins"), "DePatch.zip");
                        Log.Warn(text2);
                        File.Delete(text2);
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.DownloadFile("http://google.com/DePatch.zip", text2);
                        }
                        Log.Info("Update Sucsesful");
                        Log.Warn("Force torch restarting...");
                        return true;
                    }
                    Log.Info("Up to date.");
                }
            }
            catch (Exception value)
            {
                Log.Error(value);
                Log.Fatal("Error while updating");
            }
            return false;
        }
    }
}
