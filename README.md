<div align='center'>

# 必应壁纸信息收集器

Bing Wallpaper Information Collector

[![每日收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml)
[![区域化收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml)
[![GitHub Pages](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/deploy-pages.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/deploy-pages.yml)
[![在线预览](https://img.shields.io/badge/在线预览-GitHub%20Pages-brightgreen.svg)](https://hippiezhou.github.io/BingWallpaperCollector/)
![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

</div>

基于 C# 和 .NET 9.0 开发的多语言必应壁纸信息收集工具，支持 14 个国家/地区的本地化壁纸信息收集。

> **🤖 全自动收集**: 项目已集成 GitHub Actions，每天自动收集全球 Bing 壁纸信息，无需人工干预！

## 🌐 在线预览

**📱 立即体验**: [https://hippiezhou.github.io/BingWallpaperCollector/](https://hippiezhou.github.io/BingWallpaperCollector/)

### ✨ 网站特色

- 🎨 **现代化界面** - 响应式设计，完美适配桌面和移动设备
- 🖼️ **多视图浏览** - 画廊、国家、时间轴三种浏览模式
- 🔍 **智能搜索** - 按标题、描述、国家快速筛选
- 💾 **高清下载** - 支持UHD、4K、1080p、HD多种分辨率
- ⚡ **实时更新** - 数据与收集器同步，每日自动更新
- 📊 **详细信息** - 显示壁纸标题、描述、版权、时间等完整信息
- 🔍 **SEO优化** - 完整的搜索引擎优化，支持Google、百度等搜索收录
- 📱 **社交分享** - 支持Facebook、Twitter等社交媒体分享

### 🔗 快速链接

| 链接 | 描述 |
|------|------|
| [🏠 主网站](https://hippiezhou.github.io/BingWallpaperCollector/) | 壁纸画廊主页 |
| [📋 项目介绍](https://hippiezhou.github.io/BingWallpaperCollector/promo.html) | 项目详细介绍和宣传页面 |
| [🧪 健康检查](https://hippiezhou.github.io/BingWallpaperCollector/health.html) | 网站状态和诊断 |
| [📊 数据检查](https://hippiezhou.github.io/BingWallpaperCollector/data-check.html) | 数据完整性和加载检查 |
| [🔬 功能测试](https://hippiezhou.github.io/BingWallpaperCollector/test.html) | 详细功能测试页面 |
| [🗺️ 网站地图](https://hippiezhou.github.io/BingWallpaperCollector/sitemap.xml) | 搜索引擎站点地图 |

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

### 🌐 在线展示功能
- **GitHub Pages 网站** - 美观的在线壁纸展示平台
- **响应式设计** - 完美适配桌面、平板、手机
- **多视图模式** - 画廊视图、国家视图、时间轴视图
- **智能搜索筛选** - 按标题、描述、国家、日期筛选
- **高清预览** - 模态框高清预览和多分辨率下载
- **实时数据同步** - 与数据收集器保持同步更新
- **离线友好** - 纯静态网站，无服务器依赖

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

**📖 完整的技术文档**: [查看 src/README.md](src/README.md)

如果你是开发者，想了解：
- 系统要求和环境配置
- 代码运行和编译方法
- 配置选项和API接口
- 技术实现细节
- 开发环境设置

请查看 [src/README.md](src/README.md) 获取完整的开发者文档。

## 🤖 GitHub Actions 自动化

本项目配置了完全自动化的 GitHub Actions workflows，每天自动收集全球 Bing 壁纸信息！

### ✅ 系统状态

**🎉 完全自动化运行中！**

- ✅ 已成功收集 **14 个国家** 的壁纸信息
- ✅ 28+ JSON 文件自动生成和提交（2天数据）
- ✅ 所有图片 URL 经过验证可用
- 🌐 **GitHub Pages 在线展示** - 自动部署和更新
- 📊 **实时数据同步** - 网站数据与收集器保持同步

### 🕒 自动化 Workflows

#### 1. 每日收集 (`collect-wallpapers.yml`)

**运行时间:**

- 每天 UTC 00:30 (北京时间 08:30)
- 每天 UTC 12:30 (北京时间 20:30)

**功能:**

- 自动收集所有 14 个支持国家的当日壁纸信息
- 自动提交新数据到仓库
- 生成详细的收集报告

#### 2. 区域化收集 (`collect-regional-wallpapers.yml`)

**运行时间:**

- **亚洲地区:** UTC 22:00 (北京时间次日 06:00)
- **欧洲地区:** UTC 06:00 (中欧时间 07:00/08:00)  
- **美洲地区:** UTC 14:00 (美东时间 09:00/10:00)

**功能:**

- 根据不同时区优化收集时间
- 使用矩阵策略并行收集多个国家
- 智能重试机制，避免并发冲突
- 分地区提交数据，提高成功率

#### 3. GitHub Pages 部署 (`deploy-pages.yml`)

**运行时间:**

- 当 `docs/` 或 `archive/` 目录有更新时自动触发
- 支持手动触发部署

**功能:**

- 自动复制最新数据到网站目录
- 生成数据索引文件，优化加载速度
- 验证JSON文件完整性和网站结构
- 部署到GitHub Pages，实时更新在线展示



### 🚀 手动运行 Workflows

#### 手动触发区域化收集

1. 查看 [🤖 GitHub Actions 自动化](#-github-actions-自动化) 部分，然后进入 GitHub 仓库的 Actions 页面
2. 选择 "区域化壁纸收集" workflow
3. 点击 "Run workflow"
4. 可以自定义：
   - **目标国家列表**: 用逗号分隔的国家名称（如: China,Japan,UnitedStates）
   - **收集天数**: 1-8 天

#### 手动触发每日收集

1. 查看 [🤖 GitHub Actions 自动化](#-github-actions-自动化) 部分，然后进入 GitHub 仓库的 Actions 页面
2. 选择 "每日收集 Bing 壁纸信息" workflow
3. 点击 "Run workflow" 立即执行全球收集



### 🛠 故障排除

#### 常见问题

1. **Workflow 没有运行**

   - 检查 cron 表达式是否正确
   - 确认仓库有提交活动

2. **收集失败**

   - 检查网络连接
   - 验证 Bing API 是否可访问

3. **推送失败**
   - 确认 `GITHUB_TOKEN` 权限
   - 检查分支保护规则

#### 调试步骤

1. 手动触发 workflow 进行调试
2. 查看 Actions 日志中的详细错误信息
3. 在本地使用相同的环境变量测试

### 📊 收集报告

每次运行后，GitHub Actions 会生成详细的报告，包括：

- 📈 **数据统计**: 各国家的文件数量
- ⏰ **执行时间**: 收集开始和结束时间
- ✅ **成功状态**: 哪些国家成功收集了新数据
- 🔍 **详细日志**: 完整的执行过程



## ❓ 常见问题

### Q: 为什么某些国家的内容还是显示英文？

A: 某些国家/地区可能没有完全本地化的内容，必应会返回英文作为默认语言。

### Q: 支持更多国家吗？

A: 项目已支持必应API提供的14个主要市场，覆盖了全球主要语言和地区。如需支持其他国家，可查看[开发者文档](src/README.md)了解如何修改。

### Q: 如何实现自动化收集？

A: 项目已集成 GitHub Actions 自动化收集功能！

- 🤖 **每日自动运行** - 无需手动干预
- 🌍 **多时区优化** - 覆盖全球 14 个国家
- 📊 **智能重试机制** - 避免并发冲突
- 🔄 **自动提交数据** - 持续更新仓库

如需本地运行或开发，请查看 [开发者文档](src/README.md)。

### Q: JSON 文件可以用于什么用途？

A: 可以用于构建壁纸应用、网站展示、数据分析、或者作为其他应用的数据源。

### Q: 当前系统运行状况如何？

A: 🎉 **系统运行完全正常！** GitHub Actions 自动化流程稳定运行，数据持续更新。

### Q: 图片 URL 是否可靠？

A: 是的，项目中的所有图片 URL 都经过验证，提供UHD、4K、1080p、HD等多种分辨率，确保可以正常访问和下载。

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

- **数据收集器**: 改进C#程序的功能和性能
- **网站界面**: 优化GitHub Pages的用户体验和设计
- **文档完善**: 更新README和使用说明
- **问题反馈**: 报告bugs或提出功能建议

**开发环境设置和技术细节请查看**: [开发者文档](src/README.md)
