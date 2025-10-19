# Visual Studio 2022 中使用 EditorConfig 指南

## 📋 概述

`.editorconfig` 文件已经在项目根目录配置完成，Visual Studio 2022 会自动识别和应用这些规则。

---

## ✅ Visual Studio 2022 自动支持

### 1. **自动识别**

Visual Studio 2022 会自动识别项目中的 `.editorconfig` 文件，无需任何额外配置。

### 2. **实时生效**

打开任何 C# 文件时，EditorConfig 规则会立即生效：

-   自动缩进（4 空格）
-   自动格式化
-   代码风格提示
-   命名约定检查

---

## 🔧 Visual Studio 中的使用方法

### **方法 1: 保存时自动格式化**

1. 打开 Visual Studio 2022
2. 进入 `工具` → `选项` → `文本编辑器` → `C#` → `代码样式` → `格式设置` → `常规`
3. 勾选：
    - ✅ **保存时自动格式化文档**（Format document on save）
    - ✅ **粘贴时自动格式化**（Format on paste）

### **方法 2: 手动格式化快捷键**

| 操作       | Windows 快捷键       | macOS 快捷键   |
| ---------- | -------------------- | -------------- |
| 格式化文档 | `Ctrl + K, Ctrl + D` | `⌘ + K, ⌘ + D` |
| 格式化选区 | `Ctrl + K, Ctrl + F` | `⌘ + K, ⌘ + F` |

### **方法 3: 右键菜单**

1. 在代码编辑器中右键点击
2. 选择 `格式化文档`（Format Document）

---

## 🎯 EditorConfig 在 VS 中的功能

### **1. 代码编辑器即时反馈**

-   🟢 **绿色波浪线**: 建议（Suggestion）
-   🟡 **黄色波浪线**: 警告（Warning）
-   🔴 **红色波浪线**: 错误（Error）

### **2. 快速修复（Quick Actions）**

当看到波浪线时：

1. 将光标移到该行
2. 按 `Ctrl + .`（Windows）或 `⌘ + .`（macOS）
3. 选择建议的修复选项

示例：

```
IDE0161: Convert to file-scoped namespace
    💡 使用 file-scoped namespace
```

### **3. 错误列表窗口**

查看所有代码风格问题：

1. 菜单：`视图` → `错误列表`
2. 筛选器中选择：`消息`、`警告`、`错误`
3. 双击任意问题即可跳转到对应代码

---

## 📝 常见配置说明

### **缩进规则**

```ini
indent_style = space    # 使用空格缩进
indent_size = 4         # 4 个空格
```

### **命名空间**

```ini
csharp_style_namespace_declarations = file_scoped:warning
```

效果：强制使用 `namespace X;` 而不是 `namespace X { }`

### **using 指令**

```ini
dotnet_sort_system_directives_first = true
```

效果：`System.*` 的 using 排在前面

### **私有字段命名**

```ini
dotnet_naming_rule.private_or_internal_field_should_be_begins_with_underscore.severity = warning
```

效果：私有字段必须以 `_` 开头，如 `_logger`

---

## 🔍 查看当前生效的规则

### **在 Visual Studio 中查看**

1. 在解决方案资源管理器中找到 `.editorconfig`
2. 双击打开
3. Visual Studio 会显示带有语法高亮的配置文件

### **查看特定文件的规则**

1. 打开任意 C# 文件
2. 右键点击代码
3. 选择 `代码样式` → `配置代码样式`
4. 可以看到当前应用的所有规则

---

## ⚡ 批量格式化整个项目

### **使用命令行工具**

我已为您创建了两个脚本：

#### **1. 检查格式（不修改）**

```bash
./format.sh
```

或者手动运行：

```bash
dotnet format --verify-no-changes --verbosity diagnostic
```

#### **2. 自动修复格式**

```bash
./format-fix.sh
```

或者手动运行：

```bash
dotnet format --verbosity diagnostic
```

### **在 Visual Studio 中批量格式化**

#### 方法 A：使用扩展（推荐）

1. 安装扩展：`Code Cleanup On Save`
    - 工具 → 扩展和更新
    - 搜索 "Code Cleanup On Save"
    - 安装并重启 VS

#### 方法 B：手动批量格式化

1. 选中解决方案或项目
2. 右键 → `格式化文档`（需要安装相关扩展）

---

## 🎨 自定义 EditorConfig

### **修改规则严重性**

在 `.editorconfig` 中，每个规则都可以设置严重性：

```ini
# silent - 不显示
# suggestion - 建议（绿色波浪线）
# warning - 警告（黄色波浪线）
# error - 错误（红色波浪线）

csharp_style_namespace_declarations = file_scoped:warning
```

### **在 Visual Studio UI 中修改**

1. 菜单：`工具` → `选项`
2. 导航到：`文本编辑器` → `C#` → `代码样式`
3. 在右侧面板可以图形化配置规则
4. 点击 `从 EditorConfig 生成` 可以将当前设置保存

---

## 🚀 团队协作最佳实践

### **1. 提交 .editorconfig 到版本控制**

```bash
git add .editorconfig
git commit -m "chore: 添加 EditorConfig 配置"
git push
```

### **2. 建立 Git Hook（可选）**

创建 `.git/hooks/pre-commit` 文件：

```bash
#!/bin/bash
# 提交前自动格式化
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
    echo "❌ 代码格式不符合规范，请运行 dotnet format 修复"
    exit 1
fi
```

### **3. CI/CD 集成**

在 GitHub Actions 或其他 CI 中添加格式检查：

```yaml
- name: Check code format
  run: dotnet format --verify-no-changes
```

---

## 📚 参考资源

-   [EditorConfig 官网](https://editorconfig.org/)
-   [.NET 代码样式规则](https://learn.microsoft.com/zh-cn/dotnet/fundamentals/code-analysis/style-rules/)
-   [Visual Studio EditorConfig 支持](https://learn.microsoft.com/zh-cn/visualstudio/ide/create-portable-custom-editor-options)

---

## 🆘 常见问题

### **Q: 为什么 EditorConfig 没有生效？**

A:

1. 确保 `.editorconfig` 在解决方案根目录
2. 重启 Visual Studio
3. 检查 `工具` → `选项` → `文本编辑器` → `C#` → `代码样式` 中的设置

### **Q: 如何临时禁用某个规则？**

A: 在代码中使用指令：

```csharp
#pragma warning disable IDE0161 // 转换为文件范围的命名空间
namespace MyNamespace
{
    // ...
}
#pragma warning restore IDE0161
```

### **Q: 如何只格式化修改过的文件？**

A:

```bash
# 只格式化 Git 中有变更的文件
dotnet format --include $(git diff --name-only --diff-filter=ACM "*.cs")
```

---

## ✅ 快速开始清单

-   [ ] 确认 Visual Studio 2022 已安装（版本 17.0+）
-   [ ] `.editorconfig` 文件在项目根目录
-   [ ] 打开任意 C# 文件测试自动格式化
-   [ ] 设置"保存时自动格式化"选项
-   [ ] 运行 `dotnet format` 格式化整个项目
-   [ ] 将 `.editorconfig` 提交到 Git

---

**提示**: 第一次使用时，建议先在小范围内测试，确认规则符合团队风格后再应用到整个项目。
