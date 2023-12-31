using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace TabTimeleft;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("PluginMode")] public int PluginMode { get; set; } = 0;
}