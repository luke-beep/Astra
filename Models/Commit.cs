using Astra.Enums;
using Newtonsoft.Json;

namespace Astra.Models;

public class Commit
{
    [JsonProperty("id")]
    public int Id { get; set; } = 0;
    [JsonProperty("hash")]
    public string Hash { get; set; } = "";
    [JsonProperty("author")]
    public Author Author { get; set; } = new();
    [JsonProperty("message")]
    public string Message { get; set; } = "";
    [JsonProperty("date")]
    public DateTime Date { get; set; } = DateTime.Now;
    [JsonProperty("branch")]
    public Branch Branch { get; set; } = new();
    [JsonProperty("fileModifications")]
    public Dictionary<string, FileModificationStatus> FileModifications { get; set; } = [];
    [JsonProperty("directoryModifications")]
    public Dictionary<string, DirectoryModificationStatus> DirectoryModifications { get; set; } = [];
}