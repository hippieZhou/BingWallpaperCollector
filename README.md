# 必应壁纸信息收集器 (Bing Wallpaper Information Collector)

基于 C# 和 .NET 9.0 开发的多语言必应壁纸信息收集工具，支持 14 个国家/地区的本地化壁纸信息收集。

## 功能特性

- 🌍 **多语言支持** - 支持 14 个国家/地区的本地化内容
- 📊 **JSON 数据存储** - 保存完整的壁纸信息而非下载图片
- 🖼️ **多分辨率 URL** - 提供 UHD、4K、1080p、HD 四种分辨率链接
- 📅 **历史数据收集** - 支持收集最近 8 天的历史壁纸信息
- 📁 **智能目录结构** - 按国家和日期组织数据文件
- ⚡ **并发处理** - 支持多线程并发收集，提高效率
- 🔄 **重复检测** - 避免重复收集相同的数据
- 📋 **详细日志** - 完整的操作日志记录

## 支持的国家/地区

| 编号 | 国家/地区 | 市场代码 | 语言     |
| ---- | --------- | -------- | -------- |
| 01   | 中国      | zh-CN    | 中文     |
| 02   | 美国      | en-US    | 英文     |
| 03   | 英国      | en-GB    | 英文     |
| 04   | 日本      | ja-JP    | 日文     |
| 05   | 德国      | de-DE    | 德文     |
| 06   | 法国      | fr-FR    | 法文     |
| 07   | 西班牙    | es-ES    | 西班牙文 |
| 08   | 意大利    | it-IT    | 意大利文 |
| 09   | 俄罗斯    | ru-RU    | 俄文     |
| 10   | 韩国      | ko-KR    | 韩文     |
| 11   | 巴西      | pt-BR    | 葡萄牙文 |
| 12   | 澳大利亚  | en-AU    | 英文     |
| 13   | 加拿大    | en-CA    | 英文     |
| 14   | 印度      | en-IN    | 英文     |

## 系统要求

- .NET 9.0 或更高版本
- Windows、macOS 或 Linux
- 稳定的网络连接

## 快速开始

### 1. 克隆或下载项目

```bash
git clone <repository-url>
cd BingWallpaperCollector
```

### 2. 恢复依赖包

```bash
dotnet restore
```

### 3. 运行程序

```bash
dotnet run
```

### 4. 发布可执行文件

```bash
# 发布为独立可执行文件（包含运行时）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 发布为框架依赖（需要安装.NET运行时）
dotnet publish -c Release
```

## 项目结构

```
BingWallpaperCollector/
├── BingWallpaperCollector.csproj   # 项目文件
├── Program.cs                      # 程序入口点
├── App.cs                         # 核心业务逻辑
├── README.md                      # 说明文档
└── BingWallpaperData/            # 数据存储目录（自动创建）
    ├── China/
    │   ├── 2025-08-27/
    │   │   └── wallpaper_info.json
    │   └── 2025-08-26/
    │       └── wallpaper_info.json
    ├── Japan/
    │   └── 2025-08-27/
    │       └── wallpaper_info.json
    └── UnitedKingdom/
        ├── 2025-08-27/
        │   └── wallpaper_info.json
        └── 2025-08-26/
            └── wallpaper_info.json
```

## JSON 数据结构

每个壁纸信息 JSON 文件包含以下字段：

```json
{
  "Date": "2025-08-27",
  "Country": "Japan",
  "MarketCode": "ja-JP",
  "Title": "今日は世界湖沼の日",
  "Copyright": "ソールヴァグスヴァトン湖, デンマーク (© Anton Petrus/Getty Images)",
  "CopyrightLink": "https://www.bing.com/search?q=lake&form=hpcapt",
  "Description": "ソールヴァグスヴァトン湖, デンマーク",
  "Quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20250826_FaroeLake%22&FORM=HPQUIZ",
  "Hash": "09b70003799ebbd456070dfde0312d82",
  "ImageResolutions": [
    {
      "Resolution": "UHD",
      "Url": "https://www.bing.com/th?id=OHR.FaroeLake_JA-JP2006122961_UHD.jpg",
      "Size": "3840x2160"
    },
    {
      "Resolution": "4K",
      "Url": "https://www.bing.com/th?id=OHR.FaroeLake_JA-JP2006122961_3840x2160.jpg",
      "Size": "3840x2160"
    },
    {
      "Resolution": "1080p",
      "Url": "https://www.bing.com/th?id=OHR.FaroeLake_JA-JP2006122961_1920x1080.jpg",
      "Size": "1920x1080"
    },
    {
      "Resolution": "HD",
      "Url": "https://www.bing.com/th?id=OHR.FaroeLake_JA-JP2006122961_1920x1200.jpg",
      "Size": "1920x1200"
    }
  ],
  "CreatedAt": "2025-08-27T21:36:56.068511+08:00",
  "OriginalUrlBase": "/th?id=OHR.FaroeLake_JA-JP9172526741"
}
```

## 配置选项

程序运行时支持以下交互式配置：

### 1. 收集模式选择

- **单个国家/地区**: 选择特定国家进行数据收集
- **所有支持的国家/地区**: 收集所有 14 个国家的数据

### 2. 历史天数设置

- 支持收集 1-8 天的历史数据
- 默认收集 1 天（今日）

### 3. 并发请求数

- 支持 1-5 个并发请求
- 默认 3 个并发，平衡速度和服务器负载

### 4. JSON 格式选择

- **美化格式**: 易于阅读，文件较大
- **压缩格式**: 节省空间，文件较小

## API 接口

程序使用必应官方的壁纸 API：

- **API 地址**: `https://www.bing.com/HPImageArchive.aspx?format=js&idx={dayIndex}&n=1&mkt={marketCode}`
- **支持参数**:
  - `idx`: 天数索引（0=今天，1=昨天，最大 7）
  - `mkt`: 市场代码（如 zh-CN、en-US、ja-JP 等）
  - `n`: 获取图片数量（通常为 1）

## 使用示例

```bash
# 运行程序
dotnet run

# 交互式输出示例
=== 必应壁纸信息收集器配置 ===

请选择收集模式:
1. 单个国家/地区
2. 所有支持的国家/地区
请输入选择 (1-2) [默认: 1]: 1

支持的国家/地区:
01. China (zh-CN)
02. UnitedStates (en-US)
03. UnitedKingdom (en-GB)
04. Japan (ja-JP)
...
请选择国家/地区 (1-14) [默认: 1-中国]: 4

请输入要收集的历史天数 (1-8) [默认: 1]: 3
请输入并发请求数 (1-5) [默认: 3]: 3

请选择JSON格式:
1. 美化格式（易读）
2. 压缩格式（占用空间小）
请输入选择 (1-2) [默认: 1]: 1

# 程序执行输出
=== 开始收集必应壁纸信息 ===
配置信息:
  - 目标国家: Japan
  - 历史天数: 3 天
  - 并发请求: 3 个
  - JSON格式: 美化
================================

开始为 Japan (ja-JP) 收集 3 天的历史壁纸信息...
✓ Japan - 2025-08-27 - 今日は世界湖沼の日
✓ Japan - 2025-08-26 - 驚くべき...
✓ Japan - 2025-08-25 - 火山から...
Japan 的壁纸信息收集完成

所有壁纸信息收集完成！
数据存储目录: /path/to/BingWallpaperData
```

## 技术特点

### 多语言本地化

- 使用正确的 HTTP 请求头 (`Accept-Language`) 获取对应语言内容
- 自动处理不同国家的标题、描述和版权信息的本地化

### URL 智能处理

- 自动修正图片 URL 中的市场代码
- 根据国家代码生成一致的图片标识符
- 提供多种分辨率的图片链接

### 并发控制

- 使用信号量控制 API 请求频率
- 避免对必应服务器造成过大压力
- 支持用户自定义并发数量

## 依赖包

- `System.Text.Json`: JSON 序列化和反序列化
- `Microsoft.Extensions.Http`: HTTP 客户端工厂
- `Microsoft.Extensions.Logging`: 日志框架
- `Microsoft.Extensions.Logging.Console`: 控制台日志输出

## 常见问题

### Q: 为什么某些国家的内容还是显示英文？

A: 某些国家/地区可能没有完全本地化的内容，必应会返回英文作为默认语言。

### Q: 如何修改数据存储目录？

A: 可以在 `App.cs` 中修改 `_dataDirectory` 的初始化代码。

### Q: 支持更多国家吗？

A: 可以在 `MarketCode` 枚举中添加更多国家代码，但需要确保必应 API 支持该市场。

### Q: 如何实现自动化收集？

A: 可以配合系统的计划任务（Windows）或 cron（Linux/macOS）来实现定时运行。

### Q: JSON 文件可以用于什么用途？

A: 可以用于构建壁纸应用、网站展示、数据分析、或者作为其他应用的数据源。

## 许可证

本项目采用 MIT 许可证，详情请参阅 LICENSE 文件。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 更新日志

### v2.0.0 (当前版本)

- 🔄 **重大更新**: 从壁纸下载器转换为壁纸信息收集器
- 🌍 新增多语言支持（14 个国家/地区）
- 📊 JSON 数据存储替代图片下载
- 🖼️ 多分辨率 URL 支持（UHD、4K、1080p、HD）
- 📅 历史数据收集功能（最多 8 天）
- 📁 智能目录结构（按国家和日期组织）
- ⚡ 并发处理和性能优化
- 🎨 交互式用户界面

### v1.0.0

- 初始版本发布
- 支持必应壁纸自动下载
- 支持 4K 和 HD 分辨率
- 完整的错误处理和日志记录
