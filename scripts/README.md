# 脚本工具集

本目录包含项目开发过程中使用的各种实用脚本。

## 📋 可用脚本

### Git Hooks 配置脚本

用于配置 Git Hooks，自动验证提交信息是否符合 [Conventional Commits](../docs/ConventionalCommits.md) 规范。

#### 🪟 Windows 用户

```powershell
# 在项目根目录执行
.\scripts\setup-git-hooks.ps1
```

#### 🐧 macOS / Linux 用户

```bash
# 在项目根目录执行
./scripts/setup-git-hooks.sh
```

### 功能说明

配置完成后，当您执行 `git commit` 时：

✅ **符合规范的提交** - 正常通过

```bash
git commit -m "feat(gallery): 添加壁纸收藏功能"
# ✓ 提交信息格式正确
```

❌ **不符合规范的提交** - 会被拒绝并显示错误提示

```bash
git commit -m "add new feature"
# ✗ 提交信息格式不符合 Conventional Commits 规范
# 并显示详细的格式说明和示例
```

### 跳过验证

如果特殊情况下需要跳过验证（不推荐）：

```bash
git commit --no-verify -m "your message"
```

## 📚 相关文档

-   [Conventional Commits 提交规范](../docs/ConventionalCommits.md) - 详细的提交规范说明
-   [快速开始指南](../docs/QuickStart.md) - 项目开发环境配置

## 🔧 故障排查

### Windows 执行策略问题

如果在 Windows 上遇到执行策略错误：

```powershell
# 临时允许当前会话执行脚本
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# 然后运行脚本
.\scripts\setup-git-hooks.ps1
```

### macOS/Linux 权限问题

如果提示权限不足：

```bash
# 添加可执行权限
chmod +x scripts/setup-git-hooks.sh

# 然后运行
./scripts/setup-git-hooks.sh
```

---

**最后更新时间**：2025-10-19
