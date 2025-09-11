# 必应壁纸展示平台

这是一个基于 GitHub Pages 的静态网站，用于展示必应壁纸信息收集器收集的数据。

## 🌐 访问地址

**主站地址**: https://hippiezhou.github.io/BingWallpaperCollector/

## 📋 功能特性

- 📱 **响应式设计** - 完美适配桌面和移动设备
- 🌍 **多国家浏览** - 支持 14 个国家/地区的壁纸
- 📅 **时间轴视图** - 按日期浏览历史壁纸
- 🔍 **智能搜索** - 按标题、描述、国家筛选
- 🖼️ **高清预览** - 支持多分辨率下载
- 🎨 **现代界面** - 美观的用户界面设计

## 🚀 技术栈

- **前端**: 纯 JavaScript (ES6+)、CSS3、HTML5
- **部署**: GitHub Pages + GitHub Actions
- **数据**: 直接读取仓库中的 JSON 文件
- **样式**: 现代 CSS Grid + Flexbox 布局
- **图标**: Font Awesome 6

## 📁 项目结构

```
page/
├── index.html              # 主页
├── assets/
│   ├── css/
│   │   └── style.css       # 主样式文件
│   ├── js/
│   │   ├── app.js         # 主应用逻辑
│   │   └── data-loader.js  # 数据加载模块
│   └── images/             # 图片资源
├── health.html            # 健康监控页面
├── data-check.html        # 数据检查页面
├── debug-dates.html       # 调试页面
├── test.html              # 功能测试页面
├── promo.html             # 项目介绍页面
├── offline.html           # 离线页面
├── data-index.js          # 数据索引（自动生成）
└── _config.yml            # Jekyll配置
```

## 🔄 自动部署

网站通过 GitHub Actions 自动部署：

1. 当`main`分支的`page/`或`archive/`目录有更新时触发
2. 自动生成数据索引文件
3. 验证 HTML 和 JSON 文件完整性
4. 部署到 GitHub Pages

## 📊 数据来源

网站数据来源于同仓库的`archive/`目录：

- 支持 14 个国家/地区
- 每日自动更新
- JSON 格式存储
- 包含多分辨率图片链接

## 🛠️ 本地开发

如需本地开发，请：

1. 克隆仓库
2. 启动本地 HTTP 服务器
3. 访问`page/index.html`

```bash
# 使用Python启动本地服务器
cd page
python -m http.server 8000

# 或使用Node.js
npx http-server . -p 8000
```

## 📱 浏览器支持

- Chrome 70+
- Firefox 65+
- Safari 12+
- Edge 79+

## 📄 许可证

本项目采用 MIT 许可证，与主项目保持一致。
