using System.Text.Json.Serialization;

namespace BingWallpaperCollector.Models;

/// <summary>
/// 必应API响应类
/// </summary>
public sealed class BingApiResponse
{
  [JsonPropertyName("images")]
  public List<BingWallpaperInfo> Images { get; set; } = [];

  [JsonPropertyName("tooltips")]
  public Tooltips? Tooltips { get; set; }
}
