# Markdown_Z

简体中文 | English

---

## 简介

Markdown_Z 是一个基于 .NET 8 的 Windows WPF 轻量级 Markdown 编辑与阅读工具。它内置编辑/阅读两种模式、支持分屏实时预览、可在中文与英文界面间即时切换，并提供文件的新建/打开/保存/另存为与导出 HTML 功能。渲染引擎基于 Markdig（启用高级扩展），可良好支持表格、任务列表、脚注、定义列表、围栏代码块等常见 Markdown 特性。

## 功能特性

- 编辑模式与阅读模式一键切换，默认进入阅读模式
- 分屏预览：编辑区与渲染区并排显示
- 文件操作：新建、打开、保存、另存为
- 导出 HTML：内置简洁样式，UTF-8（无 BOM）编码
- 多语言：自动按系统语言加载，菜单可在 中文/English 间即时切换
- 快捷键：Ctrl+N（新建）、Ctrl+O（打开）、Ctrl+S（保存）
- 渲染引擎：Markdig + 高级扩展（表格/脚注/任务列表/定义列表/围栏代码块等）

## 环境要求

- 操作系统：Windows 10/11
- SDK/工具：.NET SDK 8.0 及以上，或 Visual Studio 2022（含“.NET 桌面开发”工作负载）

## 获取与运行

方法一：使用 Visual Studio

- 打开解决方案 `Markdown_Z.sln`
- 设为启动项目并 F5 运行（或“开始调试”）

方法二：使用命令行（PowerShell 或 CMD）

- 还原依赖：`dotnet restore`
- 直接运行：`dotnet run --project Markdown_Z.csproj`
- 或构建发布：
  - `dotnet build -c Release`
  - 运行可执行文件：`bin\\Release\\net8.0-windows\\Markdown_Z.exe`

方法三：从 GitHub Releases 获取发行版

- 前往本仓库的 GitHub Releases 页面
- 下载最新版本的压缩包（或安装包）
- 解压后直接运行 `Markdown_Z.exe`
- 如启动提示缺少运行时，请先安装 “.NET Desktop Runtime 8.x”

## 基本用法

- 模式切换：
  - 工具栏“阅读模式”开关或“查看”菜单的“编辑模式/阅读模式”
  - 分屏预览通过工具栏“分屏”或“查看”菜单中“双栏预览”开关启用
- 文件操作：
  - “文件”菜单包含 新建/打开/保存/另存为/导出为 HTML/退出
  - 首次运行会加载示例文本；关闭或新建/打开前如有未保存更改会提示确认
- 导出 HTML：
  - 使用 Markdig 将文档转为 HTML，并注入一段简洁内联样式
  - 导出文件编码为 UTF-8（无 BOM）

## 国际化

- 程序启动时按系统语言自动加载 `Resources/Strings.zh-CN.xaml` 或 `Resources/Strings.en-US.xaml`
- 运行时可在“语言”菜单中手动切换到 中文（简体）或 English

## 依赖

- Markdig — 高性能 Markdown 解析器
- Markdig.Wpf — 将 Markdown 渲染为 WPF FlowDocument

## 许可协议

本项目采用 GPL-3.0 许可证。详情见 `LICENSE.txt`。

---

## Overview

Markdown_Z is a lightweight Markdown editor/reader for Windows, built with WPF on .NET 8. It offers Edit/Read modes, split-pane live preview, instant Chinese/English UI switching, and file operations including New/Open/Save/Save As plus Export to HTML. Rendering uses Markdig with advanced extensions for tables, task lists, footnotes, definition lists, fenced code blocks, and more.

## Features

- One-click toggle between Edit and Read modes (defaults to Read)
- Split-view live preview (editor and rendered document side by side)
- File operations: New, Open, Save, Save As
- Export to HTML with a clean built-in style, UTF-8 (no BOM)
- Localization: auto-detected at startup; switch between 中文/English at runtime
- Shortcuts: Ctrl+N (New), Ctrl+O (Open), Ctrl+S (Save)
- Renderer: Markdig with advanced extensions (tables/footnotes/task lists/definition lists/fenced code blocks)

## Requirements

- OS: Windows 10/11
- SDK/Tools: .NET SDK 8.0+ or Visual Studio 2022 with “.NET Desktop Development” workload

## Build and Run

Option A — Visual Studio

- Open `Markdown_Z.sln`
- Set as startup project and Run (F5)

Option B — Command line

- Restore: `dotnet restore`
- Run: `dotnet run --project Markdown_Z.csproj`
- Or build and run the binary:
  - `dotnet build -c Release`
  - Run `bin\\Release\\net8.0-windows\\Markdown_Z.exe`

Option C — GitHub Releases

- Visit the repository’s GitHub Releases page
- Download the latest packaged build (zip or installer)
- Unzip and run `Markdown_Z.exe`
- If prompted for missing runtime, install “.NET Desktop Runtime 8.x”

## Usage

- Mode switching:
  - Toolbar toggle “Read Mode” or via View menu (Edit/Read)
- Enable split preview via toolbar “Split” or View → “Split Preview”
- File operations:
  - File menu includes New/Open/Save/Save As/Export as HTML/Exit
  - An example document is loaded on first run; unsaved changes prompt before closing or replacing content
- HTML export:
  - Converts Markdown to HTML via Markdig and injects a minimal inline CSS
  - Output is encoded as UTF-8 (no BOM)

## Localization

- On startup, loads `Resources/Strings.zh-CN.xaml` or `Resources/Strings.en-US.xaml` based on system UI language
- Switch languages at runtime from the “Language” menu (中文/English)

## Dependencies

- Markdig — fast Markdown parser
- Markdig.Wpf — rendering to WPF FlowDocument

## License

GPL-3.0. See `LICENSE.txt` for details.
