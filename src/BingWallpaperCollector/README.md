# 必应壁纸信息收集器 - 开发者文档

> 🚀 这是必应壁纸信息收集器的开发者和技术文档，包含代码运行、配置、API 和技术实现等详细信息。

## 📋 系统要求

- .NET 9.0 或更高版本
- Windows、macOS 或 Linux
- 稳定的网络连接

## 🚀 快速开始

### 1. 克隆或下载项目

```bash
git clone git@github.com:hippieZhou/BingWallpaperCollector.git
cd BingWallpaperCollector
```

### 2. 恢复依赖包

```bash
dotnet restore src/BingWallpaperCollector.csproj
```

### 3. 运行程序

```bash
dotnet run --project src/BingWallpaperCollector.csproj
```

### 4. 发布可执行文件

```bash
# 发布为独立可执行文件（包含运行时）
dotnet publish src/BingWallpaperCollector.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 发布为框架依赖（需要安装.NET运行时）
dotnet publish src/BingWallpaperCollector.csproj -c Release
```

## ⚙️ 配置选项

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

### 5. API 分辨率选择

- **UHD4K**: 4K Ultra HD (3840x2160) - 超高清分辨率
- **QHD2K**: 2K QHD (2560x1440) - 2K 高清分辨率
- **FullHD**: Full HD (1920x1080) - 全高清分辨率
- **HD**: HD (1280x720) - 高清分辨率
- **Standard**: 标准分辨率 (1366x768) - 标准分辨率
- 默认使用 4K Ultra HD 分辨率

## 🔌 API 接口

程序使用必应官方的壁纸 API：

- **API 地址**: `https://global.bing.com/HPImageArchive.aspx?format=js&idx={dayIndex}&n={count}&pid=hp&FORM=BEHPTB&uhd=1&uhdwidth={width}&uhdheight={height}&setmkt={marketCode}&setlang={languageCode}`
- **支持参数**:
  - `idx`: 天数索引（0=今天，1=昨天，最大 7）
  - `n`: 获取图片数量（通常为 1）
  - `pid`: 页面标识符（固定为 hp）
  - `FORM`: 表单类型（固定为 BEHPTB）
  - `uhd`: 启用 Ultra HD 支持（固定为 1）
  - `uhdwidth`: UHD 图片宽度（如 3840）
  - `uhdheight`: UHD 图片高度（如 2160）
  - `setmkt`: 市场代码（如 zh-CN、en-US、ja-JP 等）
  - `setlang`: 语言代码（如 zh、en、ja 等）

## 💡 使用示例

```bash
# 运行程序
dotnet run --project src/BingWallpaperCollector.csproj

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
数据存储目录: /path/to/archive
```

## 📝 环境变量配置

程序支持以下环境变量来实现自动化运行：

| 环境变量                | 说明                   | 可选值                                       | 默认值   |
| ----------------------- | ---------------------- | -------------------------------------------- | -------- |
| `AUTO_MODE`             | 启用自动模式           | `true`, `false`                              | `false`  |
| `COLLECT_ALL_COUNTRIES` | 收集所有国家           | `true`, `false`                              | `false`  |
| `TARGET_COUNTRY`        | 目标国家（单国家模式） | 国家名称                                     | `China`  |
| `COLLECT_DAYS`          | 收集天数               | `1-8`                                        | `1`      |
| `CONCURRENT_REQUESTS`   | 并发请求数             | `1-5`                                        | `3`      |
| `JSON_FORMAT`           | JSON 格式              | `pretty`, `compressed`                       | `pretty` |
| `API_RESOLUTION`        | API 请求分辨率         | `UHD4K`, `QHD2K`, `FullHD`, `HD`, `Standard` | `UHD4K`  |

## 🔧 本地自动化测试

要在本地测试自动模式：

```bash
# 收集所有国家（4K分辨率）
export AUTO_MODE=true
export COLLECT_ALL_COUNTRIES=true
export COLLECT_DAYS=1
export JSON_FORMAT=pretty
export API_RESOLUTION=UHD4K
dotnet run --project src/BingWallpaperCollector.csproj
```

```bash
# 测试单个国家（2K分辨率）
export AUTO_MODE=true
export COLLECT_ALL_COUNTRIES=false
export TARGET_COUNTRY=Japan
export COLLECT_DAYS=1
export API_RESOLUTION=QHD2K
dotnet run --project src/BingWallpaperCollector.csproj
```

```bash
# 测试不同分辨率设置
export AUTO_MODE=true
export TARGET_COUNTRY=China
export API_RESOLUTION=FullHD  # 可选: UHD4K, QHD2K, FullHD, HD, Standard
dotnet run --project src/BingWallpaperCollector.csproj
```

## 🔧 技术特点

### 多语言本地化

- 使用正确的 HTTP 请求头 (`Accept-Language`) 获取对应语言内容
- 自动处理不同国家的标题、描述和版权信息的本地化

### 分辨率支持

- **可配置分辨率**: 支持 UHD4K、QHD2K、FullHD、HD、Standard 等多种分辨率
- **动态 API 调用**: 根据选择的分辨率动态调整 API 参数（宽度、高度）
- **智能默认值**: 默认使用 4K Ultra HD 分辨率提供最佳图像质量
- **分辨率映射**: 自动映射分辨率名称到具体的像素尺寸

### URL 智能处理

- 自动修正图片 URL 中的市场代码
- 根据国家代码生成一致的图片标识符
- 提供多种分辨率的图片链接
- 使用增强的 API 端点支持 UHD 图片请求

### 并发控制

- 使用信号量控制 API 请求频率
- 避免对必应服务器造成过大压力
- 支持用户自定义并发数量

### 架构设计

```
src/
├── Program.cs                          # 程序入口点
├── BingWallpaperApp.cs                 # 应用程序主协调器
├── Configuration/
│   ├── AppConstants.cs                 # 应用常量配置（包含分辨率支持）
│   └── CollectionConfig.cs             # 收集配置模型（增加分辨率配置）
├── Services/
│   ├── IBingWallpaperService.cs        # 壁纸收集服务接口
│   ├── IUserConfigurationService.cs    # 用户配置服务接口
│   ├── IWallpaperStorageService.cs     # 存储服务接口
│   └── Impl/
│       ├── BingWallpaperService.cs     # 壁纸收集服务实现
│       ├── UserConfigurationService.cs # 用户配置服务实现（支持分辨率配置）
│       └── WallpaperStorageService.cs  # 存储服务实现（增强字段支持）
├── Models/
│   ├── BingApiResponse.cs              # Bing API完整响应模型
│   ├── BingWallpaperInfo.cs            # Bing API壁纸信息模型（增强字段）
│   ├── WallpaperInfoStorage.cs         # 存储数据模型（新增字段）
│   ├── ImageResolution.cs              # 图片分辨率模型
│   ├── WallpaperTimeInfo.cs            # 时间信息模型
│   └── Tooltips.cs                     # API提示文本模型（新增）
├── Enums/
│   ├── MarketCode.cs                   # 支持的市场代码
│   └── ApiResolution.cs                # API分辨率类型（新增）
├── Extensions/
│   └── EnumExtensions.cs               # 枚举扩展方法（增加分辨率扩展）
└── Converters/
    └── WallpaperTimeInfoConverter.cs   # JSON转换器
```

## ❓ 开发相关常见问题

### Q: 如何修改数据存储目录？

A: 可以在 `Configuration/AppConstants.cs` 中修改 `DataDirectoryName` 常量：

```csharp
public const string DataDirectoryName = "archive"; // 修改为你想要的目录名
```

### Q: 如何添加更多国家支持？

A: 在 `Enums/MarketCode.cs` 中添加新的国家代码：

```csharp
[Description("新国家描述")]
NewCountry,
```

然后在 `Extensions/EnumExtensions.cs` 中添加对应的语言代码映射。

### Q: 如何添加新的分辨率支持？

A: 在 `Enums/ApiResolution.cs` 中添加新的分辨率类型：

```csharp
/// <summary>
/// 8K Ultra HD - 7680x4320
/// </summary>
[Description("8K Ultra HD")]
UHD8K,
```

然后在 `Extensions/EnumExtensions.cs` 中的 `GetWidth()` 和 `GetHeight()` 方法中添加对应的像素映射：

```csharp
public static int GetWidth(this ApiResolution resolution)
{
    return resolution switch
    {
        // ... 现有映射
        ApiResolution.UHD8K => 7680,
        _ => 3840 // 默认4K
    };
}
```

### Q: 如何修改 API 请求频率？

A: 在 `Configuration/AppConstants.cs` 中修改以下常量：

```csharp
public const int MaxConcurrentDownloads = 5;    // 最大并发数
public const int DefaultConcurrentRequests = 3; // 默认并发数
```

### Q: 如何自定义 JSON 输出格式？

A: 在 `Services/Impl/WallpaperStorageService.cs` 中修改 JSON 序列化选项：

```csharp
private static readonly JsonSerializerOptions _jsonOptions = new()
{
    Converters = { new WallpaperTimeInfoConverter() },
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
```

### Q: 如何配置默认的 API 分辨率？

A: 在 `Configuration/CollectionConfig.cs` 中修改默认分辨率：

```csharp
/// <summary>
/// 默认请求分辨率
/// </summary>
public ApiResolution DefaultResolution { get; set; } = ApiResolution.UHD4K; // 可改为其他分辨率
```

或者通过环境变量设置：

```bash
export API_RESOLUTION=QHD2K  # 设置为2K分辨率
```

### Q: 如何验证图片 URL 是否可用？

A: 项目中的所有图片 URL 都经过实际 HTTP 测试验证：

```bash
# 例如测试UHD格式
curl -I "https://www.bing.com/th?id=OHR.FaroeLake_ZH-CN3977660997_UHD.jpg"
# 返回: HTTP/2 200, Content-Length: 3628316 (约3.6MB)
```

每个分辨率都确保可以正常下载使用。

## 🔧 开发环境设置

**数据收集器开发:**

```bash
# 克隆仓库
git clone https://github.com/hippieZhou/BingWallpaperCollector.git

# 进入项目目录
cd BingWallpaperCollector

# 恢复依赖
dotnet restore src/BingWallpaperCollector.csproj

# 运行程序
dotnet run --project src/BingWallpaperCollector.csproj
```

**调试和测试:**

```bash
# Debug模式运行
dotnet run --project src/BingWallpaperCollector.csproj --configuration Debug

# 构建项目
dotnet build src/BingWallpaperCollector.csproj

# 运行测试（如果有）
dotnet test src/
```

## 📖 相关文档

- [项目主页](../README.md) - 项目总览和用户指南
- [API 文档](https://www.bing.com/HPImageArchive.aspx) - Bing 官方 API
- [.NET 9.0 文档](https://docs.microsoft.com/dotnet/) - .NET 开发文档

## 🤝 开发贡献

欢迎开发者贡献代码！

### 代码规范

- 使用 C# 编码规范
- 遵循现有的项目结构
- 添加适当的注释和文档
- 确保新功能的测试覆盖

### Pull Request 流程

1. Fork 项目仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

---

> 📝 **提示**: 这是技术文档。如果你是普通用户，请查看[项目主页 README](../README.md) 获取使用指南。
