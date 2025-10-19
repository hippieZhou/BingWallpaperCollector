// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Attributes;

namespace BingWallpaperGallery.Core.Http.Enums;

/// <summary>
/// 支持的国家/地区市场代码
/// </summary>
public enum MarketCode
{
    [MarketInfo("zh-CN", "中国", "Chinese - China (Simplified)", "🇨🇳", "中国版必应壁纸，侧重中国文化、风景和节庆")]
    China,

    [MarketInfo("en-US", "美国", "English - United States", "🇺🇸", "美国版必应每日壁纸，涵盖美国本土风景和节日主题")]
    UnitedStates,

    [MarketInfo("en-GB", "英国", "English - United Kingdom", "🇬🇧", "英国版壁纸，英国文化与风光的精选集合")]
    UnitedKingdom,

    [MarketInfo("ja-JP", "日本", "Japanese - Japan", "🇯🇵", "日本地区壁纸，包含日本名胜、季节性风景")]
    Japan,

    [MarketInfo("de-DE", "德国", "German - Germany", "🇩🇪", "德国版壁纸，包含德国城市景观和自然风光")]
    Germany,

    [MarketInfo("fr-FR", "法国", "French - France", "🇫🇷", "法国版壁纸，突出法国历史遗迹及浪漫风情")]
    France,

    [MarketInfo("es-ES", "西班牙", "Spanish - Spain", "🇪🇸", "西班牙版壁纸，展现热情的西班牙风情及历史遗址")]
    Spain,

    [MarketInfo("it-IT", "意大利", "Italian - Italy", "🇮🇹", "意大利版壁纸，汇集意大利艺术古迹和风景")]
    Italy,

    [MarketInfo("ru-RU", "俄罗斯", "Russian - Russia", "🇷🇺", "俄罗斯版壁纸，展现俄罗斯广袤的自然风光")]
    Russia,

    [MarketInfo("ko-KR", "韩国", "Korean - South Korea", "🇰🇷", "韩国版壁纸，包含韩国现代与传统文化的融合")]
    SouthKorea,

    [MarketInfo("pt-BR", "巴西", "Portuguese - Brazil", "🇧🇷", "巴西版壁纸，展示热带风光和节日庆典")]
    Brazil,

    [MarketInfo("en-AU", "澳大利亚", "English - Australia", "🇦🇺", "澳大利亚版壁纸，展现澳洲独特的自然景观")]
    Australia,

    [MarketInfo("en-CA", "加拿大", "English - Canada", "🇨🇦", "加拿大版壁纸，展现枫叶国自然美景和多元文化")]
    Canada,

    [MarketInfo("en-IN", "印度", "English - India", "🇮🇳", "印度地区壁纸，反映印度地域特色与文化")]
    India
}
