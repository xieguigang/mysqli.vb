---
name: slow-query-topn-sort
overview: 修改 MySqlMonitor 的 slow query 面板：去掉 long_query_time 阈值过滤，直接收集所有非 Sleep 且有 SQL 的运行中查询，按执行时间降序排序后输出 top N（N=MaxSlowRows/-n）。-s 阈值参数保留，仅作面板 TIME 列颜色分级用。
todos:
  - id: modify-processlistreader
    content: 移除 GetSlowQueries 阈值过滤与 longQueryThreshold 形参，保留降序与 top N
    status: completed
  - id: update-program-call
    content: 更新 Program.vb 调用为 GetSlowQueries(opts.MaxSlowRows)
    status: completed
    dependencies:
      - modify-processlistreader
  - id: update-options-comment
    content: 更新 MonitorOptions.vb 中 SlowThreshold 注释为仅颜色分级
    status: completed
  - id: update-dashboard-title
    content: 修改 Dashboard.vb 面板标题去除阈值字样，保留 top N 描述
    status: completed
---

## 用户需求

修改 MySqlMonitor 的 slow query 面板显示逻辑，使其直接按查询执行时间长度降序排序后输出设置的 top N 条正在执行的查询记录，不再使用 long_query_time 阈值进行过滤。

## 产品概述

MySqlMonitor 是一个基于控制台的 MySQL 实时性能监视工具，通过 PerformanceCounter 项目读取 `SHOW PROCESSLIST` 获取慢查询信息并在 ANSI 面板中展示。本次改动调整 slow query 数据的收集与展示口径。

## 核心功能

- 收集所有非 Sleep 且有 SQL 文本的执行中查询（移除原 long_query_time 阈值过滤）。
- 对收集到的查询按执行时间（TimeSec）降序排序，取前 N 条（top N，由 `-n` 参数控制）。
- 面板标题与代码注释更新，去除"threshold"过滤语义，仅保留 top N 说明。
- `-s` 慢查询阈值参数保留，仅用于面板 TIME 列的颜色分级（绿/黄/红），不再参与过滤。

## 技术栈选择

- 语言：Visual Basic (.NET)，与现有项目一致（PerformanceCounter / MySqlMonitor 均为 VB 项目）。
- 数据来源：`SHOW PROCESSLIST` 经 `MySqli.ExecuteDataset` 读取，沿用现有 `ProcessListReader` 与 `SlowQueryInfo` 类型。
- 展示层：`Dashboard.RenderSlowQueries` 字符串构建 + ANSI 着色，沿用现有渲染模式，无新增依赖。

## 实现方案

核心策略：在 `ProcessListReader.GetSlowQueries` 中移除按 `longQueryThreshold` 的过滤分支，使所有非 Sleep、有 Info 的查询进入结果集，再按 `TimeSec` 降序排序并截断为 `maxRows`（该排序与截断逻辑已存在且正确，无需重写）。方法签名中的 `longQueryThreshold` 参数移除，调用方同步更新，避免"参数存在但不使用"的误导性。

关键技术决策：

1. 移除阈值过滤（第39行 `If timeSec >= longQueryThreshold Then ... End If` 包裹），直接 `result.Add(...)`。时间与空间复杂度仍为 O(n log n) 排序 + O(n) 遍历，与现状一致，无性能退化。
2. 移除 `longQueryThreshold` 形参：保持接口语义清晰（YAGNI/SoC），同步更新 `Program.vb` 调用与 XML 注释；`-s` 阈值继续经 `MonitorOptions.SlowThreshold` 传入 `Dashboard` 仅供颜色分级。
3. 面板标题去除 "threshold Xs" 字样，改为如 "Current Running Queries (slowest top N)"，与"无阈值过滤、按执行时间降序"的语义一致；`RenderSlowQueries` 中第325行 `Grade(..., _opts.SlowThreshold, ...)` 保持不变。

## 实现注意事项

- 延续现有 `SafeGet*` 防御式读取与 `Try/Catch` 容错，不引入新异常路径。
- `Program.vb` 调用处 `procList.GetSlowQueries(opts.SlowThreshold, opts.MaxSlowRows)` 改为 `procList.GetSlowQueries(opts.MaxSlowRows)`，参数顺序与命名同步调整。
- `-s` 命令行参数解析（`MonitorOptions.vb`）保留，仅更新其属性注释，明确"仅用于颜色分级，不再过滤"。
- 控制改动范围，仅触及 3 个文件，避免影响其他面板与计数逻辑。

## 架构设计

现有数据流：Program 主循环 → `ProcessListReader.GetSlowQueries` 收集 `List(Of SlowQueryInfo)` → `Dashboard.RenderSlowQueries` 渲染。本次仅修改收集阶段过滤条件与接口签名，渲染层与上游调度不变，保持向后兼容。

## 目录结构

```
PerformanceCounter/
└── ProcessListReader.vb   # [MODIFY] 移除 GetSlowQueries 的 longQueryThreshold 形参与第39行阈值过滤；直接收集全部非 Sleep 且有 Info 的查询；更新 XML 注释（maxRows 含义改为 "slowest first"）。保留现有降序排序与 top N 截断。
MySqlMonitor/
├── Program.vb             # [MODIFY] 第92行调用改为 procList.GetSlowQueries(opts.MaxSlowRows)，移除传入 SlowThreshold。
├── MonitorOptions.vb      # [MODIFY] 更新 SlowThreshold 属性注释，说明仅用于面板颜色分级、不再过滤；如需可微调帮助文案。
└── src/Dashboard.vb       # [MODIFY] 第289行面板标题去除 "threshold Xs"，改为仅含 top N 的描述；第325行颜色分级逻辑保持不变。
```