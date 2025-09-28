# 🤖 GitHub Actions 自动化

> 📋 **必应壁纸信息收集器** 的 GitHub Actions 工作流配置和使用说明

[![每日收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-wallpapers.yml)
[![区域化收集](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/collect-regional-wallpapers.yml)
[![GitHub Pages](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/deploy-pages.yml/badge.svg)](https://github.com/hippieZhou/BingWallpaperCollector/actions/workflows/deploy-pages.yml)

本项目配置了完全自动化的 GitHub Actions workflows，每天自动收集全球 Bing 壁纸信息！

## 📊 系统状态

**🎉 完全自动化运行中！**

- ✅ 已成功收集 **14 个国家** 的壁纸信息
- ✅ 30+ JSON 文件自动生成和提交
- ✅ 所有图片 URL 经过验证可用
- 🌐 **GitHub Pages 在线展示** - 自动部署和更新
- 📊 **实时数据同步** - 网站数据与收集器保持同步

## 🕒 自动化工作流

### 1. 每日收集 (`collect-wallpapers.yml`)

**📅 运行时间:**
- 每天 UTC 00:30 (北京时间 08:30)
- 每天 UTC 12:30 (北京时间 20:30)

**⚡ 功能特性:**
- 自动收集所有 14 个支持国家的当日壁纸信息
- 自动提交新数据到仓库
- 生成详细的收集报告
- 智能跳过已存在的数据文件

**🔧 配置参数:**
```yaml
env:
  AUTO_MODE: "true"
  COLLECT_ALL_COUNTRIES: "true"
  COLLECT_DAYS: "1"
  CONCURRENT_REQUESTS: "3"
  JSON_FORMAT: "pretty"
  API_RESOLUTION: "UHD4K"
```

### 2. 区域化收集 (`collect-regional-wallpapers.yml`)

**📅 运行时间:**
- **亚洲地区:** UTC 22:00 (北京时间次日 06:00)
- **欧洲地区:** UTC 06:00 (中欧时间 07:00/08:00)  
- **美洲地区:** UTC 14:00 (美东时间 09:00/10:00)

**⚡ 功能特性:**
- 根据不同时区优化收集时间
- 使用矩阵策略并行收集多个国家
- 智能重试机制，避免并发冲突
- 分地区提交数据，提高成功率
- 分辨率优化: 亚洲/美洲地区使用4K，欧洲地区使用2K分辨率

**🌍 地区分组:**
```yaml
# 亚洲地区
countries: ["China", "Japan", "SouthKorea", "India"]

# 欧洲地区
countries: ["UnitedKingdom", "Germany", "France", "Spain", "Italy", "Russia"]

# 美洲地区
countries: ["UnitedStates", "Canada", "Brazil", "Australia"]
```

### 3. GitHub Pages 部署 (`deploy-pages.yml`)

**📅 运行时间:**
- 当 `docs/` 或 `archive/` 目录有更新时自动触发
- 支持手动触发部署
- 每天 UTC 2:00 定时执行 (北京时间 10:00)

**⚡ 功能特性:**
- 自动复制最新数据到网站目录
- 生成数据索引文件，优化加载速度
- 验证JSON文件完整性和网站结构
- 部署到GitHub Pages，实时更新在线展示

**🔧 部署流程:**
1. **数据复制** - 将 `archive/` 复制到 `docs/archive/`
2. **索引生成** - 扫描数据文件，生成 `data-index.js`
3. **文件验证** - 检查JSON格式和关键文件
4. **Jekyll构建** - 配置GitHub Pages环境
5. **网站部署** - 发布到 https://hippiezhou.github.io/BingWallpaperCollector/

## 🚀 手动运行工作流

### 手动触发区域化收集

1. 进入 GitHub 仓库的 **Actions** 页面
2. 选择 **"区域化壁纸收集"** workflow
3. 点击 **"Run workflow"**
4. 可以自定义参数：
   - **目标国家列表**: 用逗号分隔的国家名称（如: `China,Japan,UnitedStates`）
   - **收集天数**: 1-8 天
   - **API 分辨率**: UHD4K, QHD2K, FullHD, HD, Standard

### 手动触发每日收集

1. 进入 GitHub 仓库的 **Actions** 页面
2. 选择 **"每日收集 Bing 壁纸信息"** workflow
3. 点击 **"Run workflow"** 立即执行全球收集
4. 可以自定义参数：
   - **收集天数**: 1-8 天
   - **API 分辨率**: UHD4K, QHD2K, FullHD, HD, Standard

### 手动触发页面部署

1. 进入 GitHub 仓库的 **Actions** 页面
2. 选择 **"部署到 GitHub Pages"** workflow
3. 点击 **"Run workflow"** 立即重新部署网站

## 📝 环境变量配置

所有工作流支持以下环境变量：

| 环境变量                | 说明                   | 可选值                                       | 默认值     |
| ----------------------- | ---------------------- | -------------------------------------------- | ---------- |
| `AUTO_MODE`             | 启用自动模式           | `true`, `false`                             | `true`     |
| `COLLECT_ALL_COUNTRIES` | 收集所有国家           | `true`, `false`                             | `true`     |
| `TARGET_COUNTRY`        | 目标国家（单国家模式） | 国家名称                                     | `China`    |
| `COLLECT_DAYS`          | 收集天数               | `1-8`                                       | `1`        |
| `CONCURRENT_REQUESTS`   | 并发请求数             | `1-5`                                       | `2-3`      |
| `JSON_FORMAT`           | JSON 格式              | `pretty`, `compressed`                      | `pretty`   |
| `API_RESOLUTION`        | API 请求分辨率         | `UHD4K`, `QHD2K`, `FullHD`, `HD`, `Standard` | `UHD4K`    |

## 🛠 故障排除

### ❓ 常见问题

#### 1. **Workflow 没有运行**
- 检查 cron 表达式是否正确
- 确认仓库有提交活动（GitHub会暂停长时间无活动的仓库的定时任务）
- 验证分支保护规则是否阻止了自动提交

#### 2. **收集失败**
- 检查网络连接和DNS解析
- 验证 Bing API 是否可访问（`https://www.bing.com/HPImageArchive.aspx`）
- 查看是否遇到API限流

#### 3. **推送失败**
- 确认 `GITHUB_TOKEN` 权限足够
- 检查分支保护规则
- 验证没有文件冲突

#### 4. **GitHub Pages 部署失败**
- 检查 `docs/` 目录结构
- 验证JSON文件格式正确性
- 确认Pages设置正确（从 `main` 分支的 `docs` 文件夹部署）

### 🔍 调试步骤

1. **手动触发 workflow** 进行调试
2. **查看 Actions 日志** 中的详细错误信息
3. **本地测试** 使用相同的环境变量
4. **检查仓库权限** 确认 Actions 有写入权限
5. **验证API可用性** 手动测试 Bing API 响应

### 📋 日志分析

每个workflow都会输出详细的日志信息：

**收集workflow日志包含:**
```
🚀 应用程序启动，开始初始化...
=== 开始收集必应壁纸信息 ===
配置信息:
  - 目标国家: China
  - 历史天数: 1 天
  - 并发请求: 2 个
  - JSON格式: 美化
================================
✅ China 的壁纸信息收集完成 - 共有 1 个文件
所有壁纸信息收集完成！
数据存储目录: /path/to/archive
```

**部署workflow日志包含:**
```
📊 复制统计:
  - 国家目录数: 14
  - JSON文件总数: 30
  - 总文件大小: 156K
✅ 数据索引生成完成:
  - 可用国家: 14 个
  - 可用日期: 3 个  
  - JSON文件总数: 30 个
```

## 📊 收集报告

每次运行后，GitHub Actions 会生成详细的报告，包括：

- 📈 **数据统计**: 各国家的文件数量和大小
- ⏰ **执行时间**: 收集开始和结束时间，总耗时
- ✅ **成功状态**: 哪些国家成功收集了新数据
- 🔍 **详细日志**: 完整的执行过程和API调用
- 💾 **存储信息**: 文件路径、格式、验证结果

### 示例报告

```
=== 收集完成报告 ===
📊 收集统计:
  - 目标国家数: 14
  - 成功收集: 12 个国家
  - 新增文件: 8 个
  - 跳过文件: 4 个 (已存在)
  - 失败国家: 2 个

⏰ 时间统计:
  - 开始时间: 2025-08-29 08:30:15
  - 结束时间: 2025-08-29 08:32:45  
  - 总耗时: 2分30秒
  - 平均每国家: 12.5秒

💾 存储统计:
  - 数据目录: archive/
  - JSON文件总数: 30
  - 总数据大小: 156KB
  - 最新数据: 2025-08-29
```

## 🔧 工作流配置

### Cron 表达式说明

- `'0 0,12 * * *'` - 每天UTC 0:00和12:00执行
- `'0 22 * * *'` - 每天UTC 22:00执行 (北京时间次日6:00)
- `'0 6 * * *'` - 每天UTC 6:00执行 (中欧时间7:00/8:00)
- `'0 14 * * *'` - 每天UTC 14:00执行 (美东时间9:00/10:00)
- `'0 2 * * *'` - 每天UTC 2:00执行 (北京时间10:00)

### 权限配置

```yaml
permissions:
  contents: write    # 允许修改仓库内容
  pages: write      # 允许部署到GitHub Pages  
  id-token: write   # 允许OIDC身份验证
  actions: read     # 允许读取Actions状态
```

### 并发控制

```yaml
concurrency:
  group: "data-collection"
  cancel-in-progress: false  # 不取消正在运行的任务
```

## 📚 相关链接

- **[项目主页](../README.md)** - 项目总览和用户指南
- **[开发者文档](../src/README.md)** - 技术实现和开发指南
- **[在线预览](https://hippiezhou.github.io/BingWallpaperCollector/)** - GitHub Pages网站
- **[GitHub Actions 官方文档](https://docs.github.com/actions)** - 工作流语法参考

---

> 💡 **提示**: 如果你是项目的fork用户，请确保在你的仓库设置中启用了 Actions 和 Pages 功能。
