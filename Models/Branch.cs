using Newtonsoft.Json;

namespace Astra.Models;

public class Branch
{
    [JsonProperty("id")]
    public int Id { get; set; } = 0;
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    [JsonProperty("remoteId")]
    public int RemoteId { get; set; } = 0;
}