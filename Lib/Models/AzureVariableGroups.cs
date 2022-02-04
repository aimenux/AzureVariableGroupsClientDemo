using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureVariableGroups
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<AzureVariableGroup>? VariableGroups { get; set; }
}