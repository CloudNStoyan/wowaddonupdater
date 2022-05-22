using System.IO.Compression;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AddonUpdaterAlpha
{
    public class CurseForgeClient
    {
        private RestClient RestClient { get; }
        private const string ApiKeyParameterName = "x-api-key";
        private const string BaseEndpoint = "https://api.curseforge.com";
        private UserConfig userConfig { get; }
        private UpdateManager UpdateManager { get; }

        public CurseForgeClient(UserConfig userConfig, UpdateManager updateManager)
        {
            this.userConfig = userConfig;
            this.UpdateManager = updateManager;
            this.RestClient = new RestClient();
            this.RestClient.AddDefaultHeader(ApiKeyParameterName, this.userConfig.ApiKey);
        }

        public async Task DownloadModByModId(int modId)
        {
            var request = new RestRequest(new Uri($"{BaseEndpoint}/v1/mods/{modId}"));

            var response = await this.RestClient.ExecuteAsync(request);

            if (!(response.IsSuccessful && response.Content is { Length: > 0 }))
            {
                Console.WriteLine($"Skipping '{modId}', Couldn't get data from API");
                return;
            }
            

            var jObject = JObject.Parse(response.Content);
            if (!jObject.ContainsKey("data"))
            {
                Console.WriteLine("Skipping, No data");
                return;
            }

            string? mainFileId = jObject["data"]?["mainFileId"]?.ToString();

            var latestFile = jObject["data"]?["latestFiles"]?.First(o => o["id"]?.ToString() == mainFileId);

            const string shadowlandsVersionTypeId = "517";

            var sortableGameVersions = latestFile?["sortableGameVersions"]!.Children();

            if (sortableGameVersions.Value.FirstOrDefault(x => x["gameVersionTypeId"]?.ToString() == shadowlandsVersionTypeId) == null)
            {
                var files = jObject["data"]?["latestFiles"].Children();

                latestFile = files.Cast<JToken?>()
                    .FirstOrDefault(file => file["sortableGameVersions"]
                        .Children()
                        .FirstOrDefault(x => x["gameVersionTypeId"]?.ToString().Trim() ==
                            shadowlandsVersionTypeId && x["gameVersion"].ToString()[0] == '9') != null);

                if (latestFile == null)
                {
                    Console.WriteLine($"[{jObject["data"]!["name"]}] Skipped");
                    return;
                }
            }

            var addonLog = new AddonLog
            {
                AddonName = jObject["data"]!["name"]!.ToString(),
                AddonId = modId.ToString(),
                Fingerprint = latestFile!["fileFingerprint"]!.ToString()
            };

            if (!this.UpdateManager.ShouldYouUpdateAddon(addonLog))
            {
                Console.WriteLine($"[{addonLog.AddonName}] Already up-to-date.");
                return;
            }

            string? fileUrl = latestFile?["downloadUrl"]?.ToString();

            Console.WriteLine($"[{addonLog.AddonName}] Getting download URL..");

            if (fileUrl == null)
            {
                Console.WriteLine($"[{addonLog.AddonName}] Skipping '{modId}', Download URL was null");
                return;
            }
            
            string filePath = Path.Join("./", CustomUtils.GetFileName(fileUrl));

            var downloadRequest = new RestRequest(fileUrl);

            Console.WriteLine($"[{addonLog.AddonName}] Downloading the file..");

            var data = await this.RestClient.ExecuteAsync(downloadRequest);

            if (!(data.IsSuccessful && data.RawBytes is { Length: > 0 }))
            {
                Console.WriteLine($"[{addonLog.AddonName}] Skipping '{modId}', Couldn't download file");
                return;
            }

            Console.WriteLine($"[{addonLog.AddonName}] Saving the file..");

            await File.WriteAllBytesAsync(filePath, data.RawBytes);

            Console.WriteLine($"[{addonLog.AddonName}] Extracting ZIP file to Addon Folder..");
            ZipFile.ExtractToDirectory(filePath, this.userConfig.AddonFolderPath, true);

            Console.WriteLine($"[{addonLog.AddonName}] Deleting TEMP ZIP file..");
            File.Delete(filePath);

            Console.WriteLine($"[{addonLog.AddonName}] DONE!");
        }
    }
}
