using System;
using System.IO;

namespace BunyipShow.Core
{
    public static class ConfigHelper
    {
        private const string DefaultFileName = "config.json";
        private const string AppFolderName = "BunyipShow";

        /// <summary>
        /// Returns the base AppData folder for BunyipShow.
        /// Creates it if necessary.
        /// </summary>
        public static string GetAppDataFolder()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppFolderName);

            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            return appDataFolder;
        }

        /// <summary>
        /// Returns the path to the config file in AppData.
        /// If missing, copies the default from the exe folder.
        /// </summary>
        public static string GetConfigPath()
        {
            string appDataFolder = GetAppDataFolder();
            string configPath = Path.Combine(appDataFolder, DefaultFileName);

            // If AppData config does not exist, copy default from exe folder
            if (!File.Exists(configPath))
            {
                string exeFolder = AppContext.BaseDirectory;
                string defaultConfig = Path.Combine(exeFolder, DefaultFileName);

                try
                {
                    if (File.Exists(defaultConfig))
                    {
                        File.Copy(defaultConfig, configPath, overwrite: false);
                    }
                    else
                    {
                        File.WriteAllText(configPath, "{}");
                    }
                }
                catch
                {
                    // Absolute fallback — never let config creation crash the screensaver
                    try
                    {
                        if (!File.Exists(configPath))
                            File.WriteAllText(configPath, "{}");
                    }
                    catch
                    {
                        // last resort: swallow
                    }
                }
            }

            return configPath;
        }
    }
}