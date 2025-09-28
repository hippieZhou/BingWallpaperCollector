using System.Text.Json.Serialization;

namespace BingWallpaperCollector.Models;

/// <summary>
/// 必应API界面提示文本类
/// </summary>
public sealed class Tooltips
{
  [JsonPropertyName("loading")]
  public string Loading { get; set; } = string.Empty;

  [JsonPropertyName("previous")]
  public string Previous { get; set; } = string.Empty;

  [JsonPropertyName("next")]
  public string Next { get; set; } = string.Empty;

  [JsonPropertyName("walle")]
  public string Walle { get; set; } = string.Empty;

  [JsonPropertyName("walls")]
  public string Walls { get; set; } = string.Empty;
}
