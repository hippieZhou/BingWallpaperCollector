using System.Text.Json.Serialization;

namespace BingWallpaperCollector.Models;

/// <summary>
/// 必应壁纸信息类（原始API数据）
/// </summary>
public sealed class BingWallpaperInfo
{
  [JsonPropertyName("url")]
  public string Url { get; set; } = string.Empty;

  [JsonPropertyName("urlbase")]
  public string UrlBase { get; set; } = string.Empty;

  [JsonPropertyName("copyright")]
  public string Copyright { get; set; } = string.Empty;

  [JsonPropertyName("copyrightlink")]
  public string CopyrightLink { get; set; } = string.Empty;

  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;

  [JsonPropertyName("quiz")]
  public string Quiz { get; set; } = string.Empty;

  [JsonPropertyName("wp")]
  public bool Wp { get; set; }

  [JsonPropertyName("hsh")]
  public string Hash { get; set; } = string.Empty;

  [JsonPropertyName("drk")]
  public int Drk { get; set; }

  [JsonPropertyName("top")]
  public int Top { get; set; }

  [JsonPropertyName("bot")]
  public int Bot { get; set; }

  [JsonPropertyName("hs")]
  public object[]? Hs { get; set; }

  [JsonPropertyName("startdate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonPropertyName("fullstartdate")]
  public string FullStartDate { get; set; } = string.Empty;

  [JsonPropertyName("enddate")]
  public string EndDate { get; set; } = string.Empty;
}
