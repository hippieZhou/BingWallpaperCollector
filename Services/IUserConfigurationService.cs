using BingWallpaperCollector.Configuration;

namespace BingWallpaperCollector.Services;

/// <summary>
/// 用户配置服务接口
/// </summary>
public interface IUserConfigurationService
{
  /// <summary>
  /// 获取用户配置
  /// </summary>
  /// <returns>用户配置</returns>
  CollectionConfig GetUserConfig();

  /// <summary>
  /// 从环境变量获取自动化配置
  /// </summary>
  /// <returns>自动化配置</returns>
  CollectionConfig GetAutomationConfig();

  /// <summary>
  /// 检查是否为自动模式
  /// </summary>
  /// <returns>如果是自动模式返回true</returns>
  bool IsAutoMode();
}
