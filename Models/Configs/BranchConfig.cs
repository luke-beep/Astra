using Newtonsoft.Json;

namespace Astra.Models.Configs;

public class BranchConfig
{
    [JsonProperty("branches")]
    public List<Branch> Branches { get; set; } = [];
}