using Newtonsoft.Json;

namespace AddonUpdaterAlpha
{
    public class AddonLog
    {
        [JsonProperty("addonId")]
        public string AddonId { get; set; }

        [JsonProperty("addonName")]
        public string AddonName { get; set; }

        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }
    }
}
