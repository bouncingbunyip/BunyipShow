using System;
using System.IO;
using System.Text.Json;

namespace BunyipShow.Core;

public static class ConfigLoader
{
    private const string ConfigFileName = "config.json";
    public static string ExpectedPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

    public static Config Load(string path)
    {
        if (!File.Exists(path))
        {
            Logger.Log($"Config file not found: {path}");
            // Return a default config so screensaver can still run
            return new Config();
        }

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Config>(json, options) ?? new Config();
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to load config file {path}: {ex.Message}");
            // Return a default config so screensaver can still run
            return new Config();
        }
    }
    public static Config? LoadOrNull()
    {
        try
        {
            string path = ExpectedPath;

            if (!File.Exists(path))
            {
                Logger.Log($"Config file not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var config = JsonSerializer.Deserialize<Config>(json, options);

            if (config == null)
            {
                Logger.Log($"Config file is corrupt or empty: {path}");
            }

            return config;
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to load config: {ex.Message}");
            return null;
        }
    }
}