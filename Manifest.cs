using Newtonsoft.Json;

namespace CP3.BeatSaberModdingTools.Tasks
{
    [JsonObject]
    public class Manifest
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("gameVersion")]
        public string? GameVersion { get; set; }
    }
}