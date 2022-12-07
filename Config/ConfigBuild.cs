namespace MusicCast_Control_WPF.Config;

internal class ConfigBuild
{
    public string ConfigPath { get; set; } = "Config.json";
    public ConfigTemplate Config { get; set; }

    public async Task updateConfig(string value)
    {
        var json = string.Empty;
        if (File.Exists(ConfigPath))
        {
            json = JsonConvert.SerializeObject(GenerateNewConfig(value), Formatting.Indented);
            File.WriteAllText("Config.json", json, new UTF8Encoding(false));
            await Task.Delay(200);
            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<ConfigTemplate>(json);
        }
    }

    public async Task InitializeAsync()
    {
        var json = string.Empty;

        if (!File.Exists(ConfigPath))
        {
            json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
            File.WriteAllText("Config.json", json, new UTF8Encoding(false));
            await Task.Delay(200);
        }

        json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
        Config = JsonConvert.DeserializeObject<ConfigTemplate>(json);
    }

    private ConfigTemplate GenerateNewConfig(string ip = "IP")
    {
        return new()
        {
            IP = ip
        };
    }
}