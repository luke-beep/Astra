using Newtonsoft.Json;

namespace Astra.Models.Configs;

public class CommitConfig
{
    [JsonProperty("commits")]
    public List<Commit> Commits { get; set; } = [];
}