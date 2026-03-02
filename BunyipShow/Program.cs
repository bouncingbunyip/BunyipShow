using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using BunyipShow.Core;

namespace BunyipShow
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Ensure log folder exists
            string appData = ConfigHelper.GetAppDataFolder();
            Directory.CreateDirectory(Path.Combine(appData, "log"));

            try
            {
                ApplicationConfiguration.Initialize();

                // Use AppData for config.json
                string configPath = ConfigHelper.GetConfigPath();
                var config = ConfigLoader.Load(configPath);

                if (config == null)
                {
                    Logger.Log($"Failed to load config at {configPath}. Exiting.");
                    return;
                }

                Logger.SetEnabled(config.EnableLogging);
                Logger.Log($"Config loaded successfully from {configPath}.");

                ImageLoader.InitializeFilters(config);

                var images = ImageLoader.ScanImages(config);
                string folderList = string.Join(", ", config.ImageSourceFolder);
                Logger.Log($"Found {images.Count} images in: {folderList}");

                if (config.DisplayOrder == DisplayOrder.Random && images.Count > 0)
                {
                    Logger.Log("Executing Fisher-Yates shuffle for maximum randomization.");
                    var rng = new Random();
                    int n = images.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rng.Next(n + 1);
                        var value = images[k];
                        images[k] = images[n];
                        images[n] = value;
                    }
                }
                else if (config.DisplayOrder == DisplayOrder.Sequential && images.Count > 0)
                {
                    Logger.Log("Sequential mode enabled. Sorting images alphabetically.");
                    // OrdinalIgnoreCase ensures 'Z' doesn't come before 'a' due to ASCII values
                    images.Sort(StringComparer.OrdinalIgnoreCase);
                }

                if (images.Count == 0)
                {
                    Logger.Log("No images found. Exiting.");
                    MessageBox.Show(
                        "No images found in the configured folder.",
                        "BunyipShow",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string argument = args.FirstOrDefault()?.ToLowerInvariant() ?? "/s";

                switch (argument)
                {
                    case "/c":
                        RunConfig();
                        break;

                    case "/p":
                        RunPreview(args, config, images);
                        break;

                    case "/s":
                    default:
                        RunScreensaver(config, images);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Fatal error: {ex}");
                MessageBox.Show(
                    $"Fatal error: {ex.Message}",
                    "BunyipShow Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void RunConfig()
        {
            Logger.Log("Configuration mode requested (/c).");
            MessageBox.Show(
                "BunyipShow has no configuration UI.\nEdit config.json in AppData instead.",
                "BunyipShow",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void RunScreensaver(Config config, System.Collections.Generic.List<string> images)
        {
            Logger.Log("Screensaver mode starting.");
            using var form = new SlideshowForm(config, images, false);
            Application.Run(form);
            Logger.Log("Screensaver mode ended.");
        }

        private static void RunPreview(string[] args, Config config, System.Collections.Generic.List<string> images)
        {
            Logger.Log("Preview mode requested.");

            if (args.Length < 2)
            {
                Logger.Log("Preview handle missing.");
                return;
            }

            if (!long.TryParse(args[1], out long previewHandle))
            {
                Logger.Log("Invalid preview handle.");
                return;
            }

            IntPtr previewWnd = new IntPtr(previewHandle);

            var form = new SlideshowForm(config, images, true)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None
            };

            // Use the NativeMethods class for all Win32 calls
            NativeMethods.SetParent(form.Handle, previewWnd);
            NativeMethods.GetClientRect(previewWnd, out NativeMethods.RECT rect);
            form.Size = new System.Drawing.Size(rect.Right - rect.Left, rect.Bottom - rect.Top);

            // Watchdog Timer to prevent zombies
            var watchdog = new System.Windows.Forms.Timer { Interval = 1000 };
            watchdog.Tick += (s, e) =>
            {
                if (!NativeMethods.IsWindow(previewWnd))
                {
                    Logger.Log($"Parent window {previewWnd} is gone. Cleaning up PID: {Environment.ProcessId}");
                    watchdog.Stop();
                    form.Close();
                    Application.Exit();
                }
            };
            watchdog.Start();

            form.Show();
            Application.Run();
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}