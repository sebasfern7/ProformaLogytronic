using System;
using System.IO;
using System.Text.Json;
using ProformaLogytronic.Models;

namespace ProformaLogytronic.Services
{
    public class JsonApplicationSettingsStore : IApplicationSettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static string FilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ProformaLogytronic",
                "settings.json");

        public AppSettings Load()
        {
            try
            {
                var path = FilePath;
                if (!File.Exists(path))
                    return new AppSettings();

                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            var path = FilePath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
        }
    }
}
