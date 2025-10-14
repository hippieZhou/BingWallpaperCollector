using System.ComponentModel;
using BingWallpaperCollector.Enums;

namespace BingWallpaperCollector.Extensions;

/// <summary>
/// 枚举扩展方法类
/// </summary>
public static class EnumExtensions
{
  /// <summary>
  /// 获取枚举的Description特性值
  /// </summary>
  /// <param name="marketCode">市场代码枚举</param>
  /// <returns>Description特性值或枚举名称</returns>
  public static string GetDescription(this MarketCode marketCode)
  {
    var type = marketCode.GetType();
    var memberInfo = type.GetMember(marketCode.ToString());

    if (memberInfo.Length > 0)
    {
      var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
      if (attrs.Length > 0)
      {
        return ((DescriptionAttribute)attrs[0]).Description;
      }
    }

    return marketCode.ToString();
  }

  /// <summary>
  /// 获取API分辨率枚举的Description特性值
  /// </summary>
  /// <param name="resolution">API分辨率枚举</param>
  /// <returns>Description特性值或枚举名称</returns>
  public static string GetDescription(this ApiResolution resolution)
  {
    var type = resolution.GetType();
    var memberInfo = type.GetMember(resolution.ToString());

    if (memberInfo.Length > 0)
    {
      var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
      if (attrs.Length > 0)
      {
        return ((DescriptionAttribute)attrs[0]).Description;
      }
    }

    return resolution.ToString();
  }

  /// <summary>
  /// 获取API分辨率对应的宽度
  /// </summary>
  /// <param name="resolution">API分辨率枚举</param>
  /// <returns>宽度值</returns>
  public static int GetWidth(this ApiResolution resolution)
  {
    return resolution switch
    {
      ApiResolution.UHD4K => 3840,
      ApiResolution.QHD2K => 2560,
      ApiResolution.FullHD => 1920,
      ApiResolution.HD => 1280,
      ApiResolution.Standard => 1366,
      _ => 3840 // 默认4K
    };
  }

  /// <summary>
  /// 获取API分辨率对应的高度
  /// </summary>
  /// <param name="resolution">API分辨率枚举</param>
  /// <returns>高度值</returns>
  public static int GetHeight(this ApiResolution resolution)
  {
    return resolution switch
    {
      ApiResolution.UHD4K => 2160,
      ApiResolution.QHD2K => 1440,
      ApiResolution.FullHD => 1080,
      ApiResolution.HD => 720,
      ApiResolution.Standard => 768,
      _ => 2160 // 默认4K
    };
  }
}
