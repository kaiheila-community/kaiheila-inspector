using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KaiheilaInspector
{
    public static class StorageHelper
    {
        public static string GetRootFilePath(string filename) => Path.Combine(GetRootPath(), filename);

        public static string GetSectionFolderPath(string sectionName)
        {
            string folderPath = Path.Combine(GetRootPath(), $"{sectionName}/");
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        public static string GetSectionFilePath(string sectionName, string filename) =>
            Path.Combine(GetSectionFolderPath(sectionName), filename);

        private static string GetRootPath()
        {
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage/");
            Directory.CreateDirectory(rootPath);
            return rootPath;
        }
    }

    /// <summary>
    /// Config Host.
    /// </summary>
    public sealed class ConfigHost
    {
        public ConfigHost(
            ILogger<ConfigHost> logger)
        {
            _logger = logger;

            ReloadConfig();
        }

        private void ReloadConfig()
        {
            ConfigFilePath = StorageHelper.GetRootFilePath("config.json");

            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));

                _logger.LogInformation("Config file generated.");

                Console.ReadKey();
                Environment.Exit(1);
            }

            try
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFilePath)) ?? new Config();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error when loading config.");

                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private readonly ILogger<ConfigHost> _logger;

        public string ConfigFilePath;
        public Config Config;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config
    {
        [JsonProperty("auth")]
        public ConfigAuth Auth { get; set; } = new ConfigAuth();

        [JsonProperty("port")]
        public int Port { get; set; } = 6020;

        [JsonProperty("self_id")]
        public long SelfId { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ConfigAuth
    {
        [JsonProperty("encrypt_key")]
        public string EncryptKey { get; set; } = "";

        [JsonProperty("verify_token")]
        public string VerifyToken { get; set; } = "";

        [JsonProperty("token")]
        public string Token { get; set; } = "";
    }
}
