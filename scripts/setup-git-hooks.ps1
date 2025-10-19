#!/usr/bin/env pwsh
# Git Hooks 配置脚本
# 用于配置 Conventional Commits 提交信息验证

Write-Host "🔧 正在配置 Git Hooks..." -ForegroundColor Cyan

$hooksDir = ".git/hooks"
$commitMsgHook = "$hooksDir/commit-msg"

# 确保 hooks 目录存在
if (-not (Test-Path $hooksDir)) {
    New-Item -ItemType Directory -Force -Path $hooksDir | Out-Null
}

# 创建 commit-msg hook
@'
#!/bin/sh
# Conventional Commits 验证脚本

commit_msg_file=$1
commit_msg=$(cat "$commit_msg_file")

# 定义颜色
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Conventional Commits 正则表达式
# 格式: type(scope): subject 或 type(scope)!: subject
pattern="^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\(.+\))?(!)?: .{1,72}"

if echo "$commit_msg" | grep -qE "$pattern"; then
    echo "${GREEN}✓ 提交信息格式正确${NC}"
    exit 0
else
    echo "${RED}✗ 提交信息格式不符合 Conventional Commits 规范${NC}"
    echo ""
    echo "${YELLOW}正确格式:${NC}"
    echo "  <type>(<scope>): <subject>"
    echo ""
    echo "${YELLOW}示例:${NC}"
    echo "  feat(gallery): 添加壁纸收藏功能"
    echo "  fix(database): 修复数据重复插入问题"
    echo "  docs: 更新 README 文档"
    echo ""
    echo "${YELLOW}支持的 type:${NC}"
    echo "  ✨ feat     - 新增功能"
    echo "  🐛 fix      - Bug 修复"
    echo "  📝 docs     - 文档变更"
    echo "  💄 style    - 代码格式"
    echo "  ♻️  refactor - 代码重构"
    echo "  ⚡ perf     - 性能优化"
    echo "  ✅ test     - 测试"
    echo "  📦 build    - 构建系统"
    echo "  👷 ci       - CI 配置"
    echo "  🔧 chore    - 其他杂项"
    echo "  ⏪ revert   - 回滚提交"
    echo ""
    echo "${YELLOW}您的提交信息:${NC}"
    echo "  $commit_msg"
    echo ""
    echo "${YELLOW}详细说明请查看: docs/ConventionalCommits.md${NC}"
    exit 1
fi
'@ | Out-File -Encoding UTF8 $commitMsgHook

Write-Host "✅ Git Hooks 配置完成！" -ForegroundColor Green
Write-Host ""
Write-Host "现在您的每次提交都会自动验证提交信息格式。" -ForegroundColor Gray
Write-Host "详细规范请查看: docs/ConventionalCommits.md" -ForegroundColor Gray
Write-Host ""
Write-Host "测试提交验证：" -ForegroundColor Yellow
Write-Host "  git commit -m `"feat(test): 测试提交信息验证`"" -ForegroundColor Gray
Write-Host ""
Write-Host "临时跳过验证（不推荐）：" -ForegroundColor Yellow
Write-Host "  git commit --no-verify -m `"your message`"" -ForegroundColor Gray

