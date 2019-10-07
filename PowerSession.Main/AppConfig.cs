namespace PowerSession
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public struct Config
    {
        [JsonProperty("install_id")] public string InstallId;
    }
    
    public static class AppConfig
    {
        private static readonly string HomeFolder =
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerSession");
        private static readonly string ConfigFile = Path.Join(HomeFolder, "config.json");
        private static readonly Config Config;

        public static string InstallId => Config.InstallId;

        static AppConfig()
        {
            if (!Directory.Exists(HomeFolder))
            {
                Directory.CreateDirectory(HomeFolder);
            }
            
            if (File.Exists(ConfigFile))
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile));
            }
            else
            {
                var installId = Guid.NewGuid();
                Config = new Config
                {
                    InstallId = installId.ToString()
                };
                var json = JsonConvert.SerializeObject(Config);
                File.WriteAllText(ConfigFile, json);
            }
        }
    }
}