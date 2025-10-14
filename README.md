<div align='center'>

# 必应壁纸信息收集器

Bing Wallpaper Information Collector

[![每日收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml)
[![区域化收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml)
![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

</div>

基于 C# 和 .NET 9.0 开发的多语言必应壁纸信息收集工具，支持 14 个国家/地区的本地化壁纸信息收集。

> **🤖 全自动收集**: 项目已集成 GitHub Actions，每天自动收集全球 Bing 壁纸信息，无需人工干预！

## 🌟 功能特性

### 🤖 数据收集功能

- **GitHub Actions 自动化** - 每天自动收集，多时区优化，自动提交数据
- **多语言支持** - 支持 14 个国家/地区的本地化内容
- **JSON 数据存储** - 保存完整的壁纸信息而非下载图片
- **多分辨率 URL** - 提供 UHD、4K、1080p、HD 四种分辨率链接
- **历史数据收集** - 支持收集最近 8 天的历史壁纸信息
- **智能目录结构** - 按国家和日期组织数据文件
- **并发处理** - 支持多线程并发收集，提高效率
- **重复检测** - 避免重复收集相同的数据
- **详细日志** - 完整的操作日志记录
- **双模式运行** - 支持交互式和自动化无人值守运行

## 🌍 支持的国家/地区

| 编号 | 国旗 | 国家/地区 | 市场代码 | 语言     |
| ---- | ---- | --------- | -------- | -------- |
| 01   | 🇨🇳   | 中国      | zh-CN    | 中文     |
| 02   | 🇺🇸   | 美国      | en-US    | 英文     |
| 03   | 🇬🇧   | 英国      | en-GB    | 英文     |
| 04   | 🇯🇵   | 日本      | ja-JP    | 日文     |
| 05   | 🇩🇪   | 德国      | de-DE    | 德文     |
| 06   | 🇫🇷   | 法国      | fr-FR    | 法文     |
| 07   | 🇪🇸   | 西班牙    | es-ES    | 西班牙文 |
| 08   | 🇮🇹   | 意大利    | it-IT    | 意大利文 |
| 09   | 🇷🇺   | 俄罗斯    | ru-RU    | 俄文     |
| 10   | 🇰🇷   | 韩国      | ko-KR    | 韩文     |
| 11   | 🇧🇷   | 巴西      | pt-BR    | 葡萄牙文 |
| 12   | 🇦🇺   | 澳大利亚  | en-AU    | 英文     |
| 13   | 🇨🇦   | 加拿大    | en-CA    | 英文     |
| 14   | 🇮🇳   | 印度      | en-IN    | 英文     |

## 👨‍💻 开发者指南

如果你是开发者，想了解：

- 系统要求和环境配置
- 代码运行和编译方法
- 配置选项和 API 接口
- 技术实现细节
- 开发环境设置

请查看 [src/README.md](src/README.md) 获取完整的开发者文档。

## 🤖 GitHub Actions 自动化

本项目配置了完全自动化的 GitHub Actions 工作流：

- **每日收集** - 每天两次自动收集所有国家的壁纸信息
- **区域化收集** - 按时区分地区优化收集，避免冲突

### ✅ 系统状态

**🎉 完全自动化运行中！** - 每天自动收集全球 14 个国家的壁纸信息

## ❓ 常见问题

### Q: 为什么某些国家的内容还是显示英文？

A: 某些国家/地区可能没有完全本地化的内容，必应会返回英文作为默认语言。

### Q: 支持更多国家吗？

A: 项目已支持必应 API 提供的 14 个主要市场，覆盖了全球主要语言和地区。如需支持其他国家，可查看[开发者文档](src/README.md)了解如何修改。

### Q: 如何实现自动化收集？

A: 项目已完全集成 GitHub Actions 自动化功能！

- 🤖 **每日自动运行** - 无需手动干预，多时区优化
- 🌍 **全球 14 国覆盖** - 智能重试机制，避免冲突
- 📊 **实时数据同步** - 自动提交数据到仓库

如需本地运行或开发，请查看 [开发者文档](src/README.md)。

### Q: JSON 文件可以用于什么用途？

A: 可以用于构建壁纸应用、网站展示、数据分析、或者作为其他应用的数据源。

### Q: 当前系统运行状况如何？

A: 🎉 **系统运行完全正常！** GitHub Actions 自动化流程稳定运行，数据持续更新。

### Q: 图片 URL 是否可靠？

A: 是的，项目中的所有图片 URL 都经过验证，提供 UHD、4K、1080p、HD 等多种分辨率，确保可以正常访问和下载。

## 📄 许可证

本项目采用 MIT 许可证，详情请参阅 LICENSE 文件。

## 👨‍💻 作者

**主要开发者:**

- [@hippieZhou](https://github.com/hippieZhou) - 项目创建者和维护者

**Co-Author:**

- [Cursor](https://cursor.sh/) - AI 编程助手，协助项目开发和优化

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

### 📝 贡献指南

- **数据收集器**: 改进 C#程序的功能和性能
- **数据处理**: 优化数据收集和存储功能
- **文档完善**: 更新 README 和使用说明
- **问题反馈**: 报告 bugs 或提出功能建议

**开发环境设置和技术细节请查看**: [开发者文档](src/README.md)
