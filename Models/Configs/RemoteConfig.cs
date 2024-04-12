using Newtonsoft.Json;

namespace Astra.Models.Configs;

public class RemoteConfig
{
    [JsonProperty("remote")]
    public Remote Remote { get; set; } = new();
}