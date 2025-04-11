using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using App.Domain.Utils;

namespace App.Domain.Models
{
    public class Patterns
    {
        public List<string> Include { get; set; } = [];
        public List<string> Exclude { get; set; } = [];
    }

    public class Session
    {
        public string RootPath { get; set; } = string.Empty;
        public string SelectedConfiguration { get; set; } = string.Empty;
    }

    public static class AppSettings
    {
        private const string SettingsPath = "./settings.json";
        private static Settings _current = new();
        private static readonly object Lock = new();

        public static Session Session => _current.Session;
        public static ObservableDictionary<string, Patterns> Configurations => _current.Configurations;

        public static void Load()
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                lock (Lock)
                {
                    _current = settings ?? new Settings();
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Settings file doesn't exist, using default settings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read settings on load: {ex.Message}");
            }
        }

        public static async Task SaveAsync()
        {
            try
            {
                Settings settingsToSave;
                lock (Lock)
                {
                    settingsToSave = _current;
                }
                await File.WriteAllTextAsync(SettingsPath, JsonSerializer.Serialize(settingsToSave));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public static void UpdateSession(Action<Session> updateAction)
        {
            lock (Lock)
            {
                updateAction(_current.Session);
            }
        }

        public static void UpdateConfigurations(Action<ObservableDictionary<string, Patterns>> updateAction)
        {
            lock (Lock)
            {
                updateAction(_current.Configurations);
            }
        }

        private class Settings
        {
            public Session Session { get; set; } = new();
            public ObservableDictionary<string, Patterns> Configurations { get; set; } = new();
        }
    }
}