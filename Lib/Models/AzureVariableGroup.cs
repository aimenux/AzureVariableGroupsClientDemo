using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureVariableGroup
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonProperty("variables")]
    [JsonPropertyName("variables")]
    public Dictionary<string, AzureVariable>? Variables { get; set; }
}