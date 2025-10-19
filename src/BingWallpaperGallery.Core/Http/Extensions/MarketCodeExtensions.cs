// Copyright (c) hippieZhou. All rights reserved.

using System.Reflection;
using BingWallpaperGallery.Core.Http.Attributes;
using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.Http.Extensions;

/// <summary>
/// MarketCode 扩展方法
/// </summary>
public static class MarketCodeExtensions
{
    /// <summary>
    /// 获取市场信息特性
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>市场信息特性</returns>
    private static MarketInfoAttribute GetMarketInfo(this MarketCode marketCode)
    {
        var fieldInfo = marketCode.GetType().GetField(marketCode.ToString());
        return fieldInfo?.GetCustomAttribute<MarketInfoAttribute>();
    }

    /// <summary>
    /// 获取市场代码
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>市场代码字符串</returns>
    public static string GetMarketCode(this MarketCode marketCode)
    {
        return marketCode.GetMarketInfo()?.Code ?? marketCode.ToString();
    }

    /// <summary>
    /// 获取市场名称
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>市场名称</returns>
    public static string GetMarketName(this MarketCode marketCode)
    {
        return marketCode.GetMarketInfo()?.Name ?? marketCode.ToString();
    }

    /// <summary>
    /// 获取市场描述
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>市场描述</returns>
    public static string GetMarketDescription(this MarketCode marketCode)
    {
        return marketCode.GetMarketInfo()?.Description ?? marketCode.ToString();
    }

    /// <summary>
    /// 获取市场国旗
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>国旗表情符号</returns>
    public static string GetMarketFlag(this MarketCode marketCode)
    {
        return marketCode.GetMarketInfo()?.Flag ?? "🏳️";
    }

    /// <summary>
    /// 获取市场备注
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>备注信息</returns>
    public static string GetMarketNote(this MarketCode marketCode)
    {
        return marketCode.GetMarketInfo()?.Note ?? "必应每日壁纸";
    }

    /// <summary>
    /// 从市场代码获取语言代码
    /// </summary>
    /// <param name="marketCode">市场代码</param>
    /// <returns>语言代码</returns>
    public static string GetLanguageCodeFromMarket(this MarketCode marketCode)
    {
        return marketCode switch
        {
            MarketCode.China => "zh-CN",
            MarketCode.UnitedStates => "en-US",
            MarketCode.UnitedKingdom => "en-GB",
            MarketCode.Japan => "ja-JP",
            MarketCode.Germany => "de-DE",
            MarketCode.France => "fr-FR",
            MarketCode.Spain => "es-ES",
            MarketCode.Italy => "it-IT",
            MarketCode.Russia => "ru-RU",
            MarketCode.SouthKorea => "ko-KR",
            MarketCode.Brazil => "pt-BR",
            MarketCode.Australia => "en-AU",
            MarketCode.Canada => "en-CA",
            MarketCode.India => "en-IN",
            _ => "en-US" // 默认使用英语
        };
    }

    public static MarketCode GetMarketFromLanguageCode(this string languageCode)
    {
        return languageCode switch
        {
            "zh-CN" => MarketCode.China,
            "en-US" => MarketCode.UnitedStates,
            "en-GB" => MarketCode.UnitedKingdom,
            "ja-JP" => MarketCode.Japan,
            "de-DE" => MarketCode.Germany,
            "fr-FR" => MarketCode.France,
            "es-ES" => MarketCode.Spain,
            "it-IT" => MarketCode.Italy,
            "ru-RU" => MarketCode.Russia,
            "ko-KR" => MarketCode.SouthKorea,
            "pt-BR" => MarketCode.Brazil,
            "en-AU" => MarketCode.Australia,
            "en-CA" => MarketCode.Canada,
            "en-IN" => MarketCode.India,
            _ => MarketCode.UnitedStates // 默认使用美国市场
        };
    }

    /// <summary>
    /// 从市场代码获取简化语言代码 (用于API的setlang参数)
    /// </summary>
    public static string GetSimpleLanguageCode(this MarketCode marketCode)
    {
        return marketCode switch
        {
            MarketCode.China => "zh",
            MarketCode.UnitedStates or
            MarketCode.UnitedKingdom or
            MarketCode.Australia or
            MarketCode.Canada or
            MarketCode.India => "en",
            MarketCode.Japan => "ja",
            MarketCode.Germany => "de",
            MarketCode.France => "fr",
            MarketCode.Spain => "es",
            MarketCode.Italy => "it",
            MarketCode.Russia => "ru",
            MarketCode.SouthKorea => "ko",
            MarketCode.Brazil => "pt",
            _ => "en" // 默认使用英语
        };
    }
}
