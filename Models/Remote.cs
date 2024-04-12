using Newtonsoft.Json;

namespace Astra.Models;

public class Remote
{
    [JsonProperty("id")]
    public int Id { get; set; } = 0;
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    [JsonProperty("path")]
    public string Path { get; set; } = "";
    [JsonProperty("branchIds")]
    public List<int> BranchIds { get; set; } = [];
    [JsonProperty("defaultBranchId")]
    public int DefaultBranchId { get; set; }
}