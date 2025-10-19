// Copyright (c) hippieZhou. All rights reserved.

namespace BingWallpaperGallery.Core.Http.Attributes;

/// <summary>
/// 市场信息特性
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="code">市场代码</param>
/// <param name="name">市场名称</param>
/// <param name="description">市场描述</param>
/// <param name="flag">国旗表情符号</param>
/// <param name="note">备注信息</param>
[AttributeUsage(AttributeTargets.Field)]
public class MarketInfoAttribute(string code, string name, string description, string flag, string note) : Attribute
{
    /// <summary>
    /// 市场代码
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// 市场名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 市场描述
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// 国旗表情符号
    /// </summary>
    public string Flag { get; } = flag;

    /// <summary>
    /// 备注信息
    /// </summary>
    public string Note { get; } = note;
}
