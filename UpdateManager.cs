using Newtonsoft.Json;

namespace AddonUpdaterAlpha
{
    public class UpdateManager
    {
        private const string LatestUpdatesFilePath = "./latestUpdates.json";
        private List<AddonLog> LatestUpdates { get; }
        public UpdateManager()
        {
            if (!File.Exists(LatestUpdatesFilePath))
            {
                this.LatestUpdates = new List<AddonLog>();
                return;
            }

            try
            {
                var logs = JsonConvert.DeserializeObject<AddonLog[]>(File.ReadAllText(LatestUpdatesFilePath));

                if (logs == null)
                {
                    this.LatestUpdates = new List<AddonLog>();
                    return;
                }

                this.LatestUpdates = logs.ToList();
            }
            catch
            {
                this.LatestUpdates = new List<AddonLog>();
            }
        }

        public bool ShouldYouUpdateAddon(AddonLog log)
        {
            var addonLog = this.LatestUpdates.FirstOrDefault(x => x.AddonId == log.AddonId);

            if (addonLog == null)
            {
                this.LatestUpdates.Add(log);
                File.WriteAllText(LatestUpdatesFilePath, JsonConvert.SerializeObject(this.LatestUpdates));
                return true;
            }

            if (addonLog.Fingerprint.Trim() == log.Fingerprint.Trim())
            {
                return false;
            }

            addonLog.Fingerprint = log.Fingerprint;
            File.WriteAllText(LatestUpdatesFilePath, JsonConvert.SerializeObject(this.LatestUpdates));
            return true;
        }
    }
}
