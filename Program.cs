using AddonUpdaterAlpha;
using Newtonsoft.Json;

const string userConfigFilePath = "./userconfig.json";

if (!File.Exists(userConfigFilePath))
{
    throw new Exception($"Cound't find user config at '{userConfigFilePath}'");
}

var config = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(userConfigFilePath));

if (config == null)
{
    throw new Exception("User config was faulty");
}

var updateManager = new UpdateManager();

var curseForgeclient = new CurseForgeClient(config, updateManager);

foreach (int modId in config.ModIds)
{
    await curseForgeclient.DownloadModByModId(modId);
}

Console.WriteLine("All AddOns are updated!");
Console.ReadKey(true);