using System.ComponentModel;

namespace BingWallpaperCollector.Enums;

/// <summary>
/// 支持的国家/地区市场代码
/// </summary>
public enum MarketCode
{
  [Description("zh-CN")]
  China,

  [Description("en-US")]
  UnitedStates,

  [Description("en-GB")]
  UnitedKingdom,

  [Description("ja-JP")]
  Japan,

  [Description("de-DE")]
  Germany,

  [Description("fr-FR")]
  France,

  [Description("es-ES")]
  Spain,

  [Description("it-IT")]
  Italy,

  [Description("ru-RU")]
  Russia,

  [Description("ko-KR")]
  SouthKorea,

  [Description("pt-BR")]
  Brazil,

  [Description("en-AU")]
  Australia,

  [Description("en-CA")]
  Canada,

  [Description("en-IN")]
  India
}
