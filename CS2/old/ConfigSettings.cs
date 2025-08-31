using Newtonsoft.Json.Linq;

namespace AliceInCradle.Config
{
    public class ConfigSettings
    {
        public float HpReductionMultiplier { get; private set; }
        public float MpReductionMultiplier { get; private set; }
        public float EpReductionMultiplier { get; private set; }
        public int CheckIntervalMs { get; private set; }
        public int ReductionValue { get; private set; }
        public int FireMode { get; private set; }
        public int Lowest { get; private set; }
        public int Hero { get; private set; }
        public int HoldMs { get; private set; }
        public int EroH { get; private set; }
        public int MaxChange { get; private set; }

        public void UpdateFromJson(JObject config)
        {
            HpReductionMultiplier = (float)config["hpReductionMultiplier"];
            MpReductionMultiplier = (float)config["mpReductionMultiplier"];
            EpReductionMultiplier = (float)config["epReductionMultiplier"];
            CheckIntervalMs = (int)config["CheckIntervalMs"];
            ReductionValue = (int)config["ReductionValue"];
            FireMode = (int)config["FireMode"];
            Lowest = (int)config["lowest"];
            Hero = (int)config["Hero"];
            HoldMs = (int)config["holdMs"];
            EroH = (int)config["eroH"];
            MaxChange = (int)config["maxChange"];
        }
    }
}
