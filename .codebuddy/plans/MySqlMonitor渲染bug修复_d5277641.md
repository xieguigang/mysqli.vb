---
name: MySqlMonitor渲染bug修复
overview: 修复 MySqlMonitor 命令行仪表盘的四类渲染问题：未铺满带鱼屏、右框错位、ESC乱码、Slow Queries 顺序，使渲染基于可见宽度且填满终端。
todos:
  - id: fix-width
    content: 修改 Dashboard 构造函数，移除 140 宽度上限并适配带鱼屏
    status: completed
  - id: fix-pad
    content: 将 Pad 改为基于可见长度，复用 VisibleLen/TruncateVisible 修复对齐与乱码
    status: completed
    dependencies:
      - fix-width
  - id: reorder-slow
    content: 调整 Render 顺序，将 Slow Queries 移到最后渲染
    status: completed
    dependencies:
      - fix-pad
  - id: verify
    content: 构建并核对各面板右框对齐与无 ESC 乱码
    status: completed
    dependencies:
      - reorder-slow
---

## 用户需求

修复命令行 MySQL 性能监视工具 MySqlMonitor 的渲染缺陷。该工具以纯 ANSI 转义序列在控制台渲染全屏仪表盘。

## 产品概述

MySqlMonitor 是一个运行于命令行、对 MySQL 进行实时性能监视的工具模块。当前渲染存在 4 个可视问题，需要修正以在宽屏（带鱼屏）上获得正确、铺满、对齐且无乱码的显示效果。

## 核心特性

- 窗口宽度铺满终端：移除宽上限限制，使仪表盘在带鱼屏等超宽终端上横向铺满，不再只显示一小块。
- 右边框对齐：所有面板（标题行、内容行、结束行、顶栏）的右侧边框 `│` 精确对齐到统一列，消除右端错位。
- 消除 ESC 乱码：在含 ANSI 颜色转义的文本上按"可见长度"进行填充/截断，避免转义序列被切断后漏出 `Esc[` 类乱码（Query Throughput 等处）。
- Slow Queries 移到最后：将「Current Slow Queries」面板的渲染顺序调整到最后，位于「History Trends」面板之后。

## 技术栈

- 语言：VB.NET（.NET Framework / .NET，基于现有项目）
- 渲染方式：纯 ANSI 转义序列（无第三方 TUI 库），字符串构建后单次写入控制台
- 修改范围：仅 `MySqlMonitor/src/Dashboard.vb`（渲染逻辑），不涉及数据采集、连接或 `Program.vb`

## 实现方案

### 总体策略

针对 4 个 bug，集中在 `Dashboard.vb` 做三处修复：宽度上限、可见长度填充、渲染顺序。已确认 `Ansi.vb` 中 `VisibleLen`/`TruncateVisible`/`PadRaw`（第 445-491 行）已实现基于可见长度的正确计算，可直接复用，无需新增重复逻辑。

### 关键技术决策

1. **宽度铺满（问题1）**：构造函数（第 25-28 行）中 `_width = Math.Min(Console.BufferWidth, 140)` 把宽度硬限在 140。改为 `_width = Console.BufferWidth`（带下限保护，如 `Math.Max(80, Console.BufferWidth)`），并考虑回退到 `Console.WindowWidth` 当 `BufferWidth` 不可用（<=0）时。这样超宽终端可直接铺满。保留 `colW = (w - colGap) \ 2` 的双列布局自动适配。
2. **可见长度填充（问题2、3）**：现 `Pad`（第 56-59 行）用 `s.Length` 计长，但 `s` 含 ANSI 转义码，导致填充错位与截断乱码。`MergeColumns` 已正确用 `PadRaw`/`VisibleLen`。统一方案：将 `Pad` 改为基于可见长度的版本（复用 `VisibleLen` 与 `TruncateVisible`，等价于把现有 `PadRaw` 语义推广到 `Header`/`PanelStart`/`PanelLine`）。具体：

- `Pad(s, width)` 内部改用 `VisibleLen(s)` 判断，`>= width` 时用 `TruncateVisible(s, width)` 截断，`< width` 时尾部补空格。这样转义序列不计入可见宽度，右框 `│` 对齐，且不会切断转义序列。
- `RenderHeader`（第 125 行）、`PanelStart` 标题行（第 145 行）、`PanelLine`（第 163 行）调用的 `Pad` 自动受益，无需各自改动。

3. **渲染顺序（问题4）**：`Render` 方法（第 96-100 行）当前为 RenderSlowQueries 在前、RenderTrends 在后。调整为先 `RenderTrends`，后 `RenderSlowQueries`，使 Slow Queries 置于最后。

### 性能与可靠性

- 渲染为纯字符串构建 + 单次写入，`VisibleLen` 为 O(n) 线性扫描，面板行数有限（数十行），开销可忽略，不会引入性能瓶颈。
- 宽度读取仅在构造函数做一次（`Console.BufferWidth`），渲染循环不重复读取，避免每帧系统调用。
- 保留回退：若 `Console.BufferWidth <= 0`（非 TTY/重定向），回退到 `WindowWidth` 或默认 80，避免异常或零宽崩溃。

### 避免技术债务

- 复用既有 `VisibleLen`/`TruncateVisible`/`PadRaw`，不重复实现可见长度算法；仅将 `Pad` 调整为与其一致，消除「部分用 `Pad`（错误）部分用 `PadRaw`（正确）」的不一致。
- 不触碰数据采集、连接、进程监控逻辑，控制改动爆炸半径。

## 实现注意

- `Pad` 被多处调用（Header、PanelStart、PanelLine），统一修正后需确认这些调用点的文本均含 ANSI 转义（如 `Ansi.Reset()`、`Ansi.Fg(...)`），改为可见长度填充后右框对齐正确。
- `MergeColumns` 已正确，无需改动；改动 `Pad` 后双列合并路径（`colW < w` 分支）与单列回退路径（`colW >= w`）均应验证边框对齐。
- 保留 `screen.txt` 作为诊断参考，不修改其内容（其乱码源于旧 bug，修复后重新捕获即可验证）。

## 架构设计

仅修改 `Dashboard` 类的宽度初始化与 `Pad` 辅助函数，以及 `Render` 的调用顺序。模块依赖关系不变：

- `Program.vb` → `Dashboard.Render(...)` → 各 `RenderXxx` 子过程 → `Ansi` 模块（已具备可见长度工具）。
数据流不变：采集数据 → `Render` 构建字符串 → `Console.Out.Write` 单次刷出。

## 目录结构

```
MySqlMonitor/
└── src/
    └── Dashboard.vb   # [MODIFY] 1) 构造函数移除 140 宽度上限，改为取 Console.BufferWidth（带下限回退）；
                       #           2) 将 Pad(s,width) 改为基于可见长度（复用 VisibleLen/TruncateVisible），
                       #              修复右框对齐与 ESC 乱码；
                       #           3) Render 中调整顺序：先 RenderTrends 后 RenderSlowQueries。
```

（其余文件 `Ansi.vb`、`Program.vb`、`MonitorOptions.vb` 等无需修改。）

## 关键代码结构

需修改的核心辅助函数签名（等价现有 `PadRaw` 语义，可直接复用 `VisibleLen`/`TruncateVisible`）：

```
' 现有（保留）：基于可见长度的填充，已被 MergeColumns 使用
Private Shared Function PadRaw(s As String, width As Integer) As String

' 修改为基于可见长度（原 Pad 用 s.Length，存在 bug）：
Private Shared Function Pad(s As String, width As Integer) As String
    ' 使用 VisibleLen(s) 计长；>=width 用 TruncateVisible(s,width)；否则尾部补空格
End Function
```