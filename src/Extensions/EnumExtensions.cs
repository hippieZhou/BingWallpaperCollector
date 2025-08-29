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
}
