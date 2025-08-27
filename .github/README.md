# GitHub Actions 自动化收集

本项目配置了简洁高效的 GitHub Actions workflows 来自动收集 Bing 壁纸信息。

## 🤖 自动化 Workflows

### 1. 每日收集 Bing 壁纸信息 (`collect-wallpapers.yml`)

**运行时间:**
- 每天 UTC 00:30 (北京时间 08:30)
- 每天 UTC 12:30 (北京时间 20:30)

**功能:**
- 自动收集所有 14 个支持国家的当日壁纸信息
- 自动提交新数据到仓库
- 生成详细的收集报告

### 2. 区域化壁纸收集 (`collect-regional-wallpapers.yml`)

**运行时间:**
- **亚洲地区:** UTC 22:00 (北京时间次日 06:00)
- **欧洲地区:** UTC 06:00 (中欧时间 07:00/08:00)
- **美洲地区:** UTC 14:00 (美东时间 09:00/10:00)

**功能:**
- 根据不同时区优化收集时间
- 使用矩阵策略并行收集多个国家
- 智能重试机制，避免并发冲突
- 分地区提交数据，提高成功率

## ✅ 系统状态

**🎉 完全自动化运行中！**
- ✅ 已成功收集 **14个国家** 的壁纸信息
- ✅ 覆盖 **2025-08-20 至 2025-08-27** 多个日期
- ✅ 110+ JSON文件自动生成和提交
- ✅ 所有图片URL经过验证可用

## 🎯 支持的收集国家

| 地区   | 国家     | 市场代码 |
| ------ | -------- | -------- |
| 亚洲   | 中国     | zh-CN    |
| 亚洲   | 日本     | ja-JP    |
| 亚洲   | 韩国     | ko-KR    |
| 亚洲   | 印度     | en-IN    |
| 欧洲   | 英国     | en-GB    |
| 欧洲   | 德国     | de-DE    |
| 欧洲   | 法国     | fr-FR    |
| 欧洲   | 西班牙   | es-ES    |
| 欧洲   | 意大利   | it-IT    |
| 欧洲   | 俄罗斯   | ru-RU    |
| 美洲   | 美国     | en-US    |
| 美洲   | 加拿大   | en-CA    |
| 美洲   | 巴西     | pt-BR    |
| 大洋洲 | 澳大利亚 | en-AU    |

## 📝 环境变量配置

程序支持以下环境变量来实现自动化运行：

| 环境变量                | 说明                   | 可选值                 | 默认值   |
| ----------------------- | ---------------------- | ---------------------- | -------- |
| `AUTO_MODE`             | 启用自动模式           | `true`, `false`        | `false`  |
| `COLLECT_ALL_COUNTRIES` | 收集所有国家           | `true`, `false`        | `false`  |
| `TARGET_COUNTRY`        | 目标国家（单国家模式） | 国家名称               | `China`  |
| `COLLECT_DAYS`          | 收集天数               | `1-8`                  | `1`      |
| `CONCURRENT_REQUESTS`   | 并发请求数             | `1-5`                  | `3`      |
| `JSON_FORMAT`           | JSON 格式              | `pretty`, `compressed` | `pretty` |

## 🚀 手动运行 Workflows

### 手动触发区域化收集

1. 进入 GitHub 仓库的 Actions 页面
2. 选择 "区域化壁纸收集" workflow
3. 点击 "Run workflow"
4. 可以自定义：
   - **目标国家列表**: 用逗号分隔的国家名称（如: China,Japan,UnitedStates）
   - **收集天数**: 1-8 天

### 手动触发每日收集

1. 进入 GitHub 仓库的 Actions 页面
2. 选择 "每日收集 Bing 壁纸信息" workflow
3. 点击 "Run workflow" 立即执行全球收集

## 📊 收集报告

每次运行后，GitHub Actions 会生成详细的报告，包括：

- 📈 **数据统计**: 各国家的文件数量
- ⏰ **执行时间**: 收集开始和结束时间
- ✅ **成功状态**: 哪些国家成功收集了新数据
- 🔍 **详细日志**: 完整的执行过程

## 📁 数据组织结构

收集的数据将自动保存到 `BingWallpaperData/` 目录：

```
BingWallpaperData/
├── China/
│   ├── 2025-01-15/
│   │   └── wallpaper_info.json
│   └── 2025-01-16/
│       └── wallpaper_info.json
├── Japan/
│   └── 2025-01-15/
│       └── wallpaper_info.json
└── UnitedStates/
    └── 2025-01-15/
        └── wallpaper_info.json
```

## 🔧 本地测试

要在本地测试自动模式：

```bash
# 设置环境变量
export AUTO_MODE=true
export COLLECT_ALL_COUNTRIES=true
export COLLECT_DAYS=1
export JSON_FORMAT=pretty

# 运行程序
dotnet run
```

或者测试单个国家：

```bash
export AUTO_MODE=true
export COLLECT_ALL_COUNTRIES=false
export TARGET_COUNTRY=Japan
export COLLECT_DAYS=1

dotnet run
```

## 📋 Workflow 状态

你可以在 GitHub 仓库的 Actions 页面查看：

- ✅ 成功执行的 workflows
- ❌ 失败的执行及错误原因
- 📊 详细的执行报告和统计

## 🛠 故障排除

### 常见问题

1. **Workflow 没有运行**

   - 检查 cron 表达式是否正确
   - 确认仓库有提交活动

2. **收集失败**

   - 检查网络连接
   - 验证 Bing API 是否可访问

3. **推送失败**
   - 确认 `GITHUB_TOKEN` 权限
   - 检查分支保护规则

### 调试步骤

1. 手动触发 workflow 进行调试
2. 查看 Actions 日志中的详细错误信息
3. 在本地使用相同的环境变量测试

## 🤝 贡献

欢迎改进自动化流程！可以：

1. 优化收集时间安排
2. 添加更多国家支持
3. 改进错误处理
4. 增强报告功能

---

_所有 workflows 都配置为自动运行，无需手动干预。如有问题，请查看 Actions 日志或提交 Issue。_
