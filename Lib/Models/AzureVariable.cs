using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureVariable
{
    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonProperty("isSecret")]
    [JsonPropertyName("isSecret")]
    public bool IsSecret { get; set; }

    [JsonProperty("isReadOnly")]
    [JsonPropertyName("isReadOnly")]
    public bool IsReadOnly { get; set; }
}