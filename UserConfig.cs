using Newtonsoft.Json;

namespace AddonUpdaterAlpha
{
    public class UserConfig
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("addonFolderPath")]
        public string AddonFolderPath { get; set; }

        [JsonProperty("modIds")]
        public int[] ModIds { get; set; }
    }
}