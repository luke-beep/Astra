using Newtonsoft.Json;

namespace Astra.Models;

public class Author
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    [JsonProperty("email")]
    public string Email { get; set; } = "";
}