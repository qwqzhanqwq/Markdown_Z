using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Markdig;
using Markdig.Wpf;
using Microsoft.Win32;

namespace Markdown_Z;

/// <summary>
/// 主窗口：支持编辑/阅读模式、分屏预览、导出 HTML、双语界面。
/// 注：本文件内注释均为中文。
/// </summary>
public partial class MainWindow : Window
{
    // 当前文件路径（为空表示未保存过）
    private string? _currentFilePath;

    // 文本是否有未保存修改
    private bool _isDirty;

    // Markdig 渲染流水线（启用常用扩展）
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    // 是否为阅读模式（true=阅读；false=编辑）
    private bool _isReadMode;

    // 是否开启分屏预览
    private bool _isSplitPreview;

    public MainWindow()
    {
        InitializeComponent();

        // 语言初始化
        InitializeLanguageBySystem();

        // 默认进入阅读模式
        SetMode(isReadMode: true);

        // 载入示例文本
        LoadSample();

        // 绑定快捷键
        BindShortcutKeys();

        // 初始标题
        UpdateWindowTitle();
    }

    /// <summary>
    /// 绑定常用快捷键：新建、打开、保存
    /// </summary>
    private void BindShortcutKeys()
    {
        var newCmd = new RoutedUICommand("New", "New", typeof(MainWindow));
        var openCmd = new RoutedUICommand("Open", "Open", typeof(MainWindow));
        var saveCmd = new RoutedUICommand("Save", "Save", typeof(MainWindow));

        CommandBindings.Add(new CommandBinding(newCmd, (_, __) => NewFile_Click(this, new RoutedEventArgs())));
        CommandBindings.Add(new CommandBinding(openCmd, (_, __) => OpenFile_Click(this, new RoutedEventArgs())));
        CommandBindings.Add(new CommandBinding(saveCmd, (_, __) => SaveFile_Click(this, new RoutedEventArgs())));

        InputBindings.Add(new KeyBinding(newCmd, Key.N, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(openCmd, Key.O, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(saveCmd, Key.S, ModifierKeys.Control));
    }

    /// <summary>
    /// 切换编辑/阅读模式
    /// </summary>
    private void SetMode(bool isReadMode)
    {
        _isReadMode = isReadMode;

        // 按分屏状态设置可见性
        if (_isSplitPreview)
        {
            // 分屏：编辑器 + 阅读器（FlowDocument 预览）
            EditorTextBox.Visibility = Visibility.Visible;
            ReaderViewer.Visibility = Visibility.Visible;
            MainSplitter.Visibility = Visibility.Visible;
            RenderMarkdownToViewer();
        }
        else
        {
            // 单栏：二选一
            MainSplitter.Visibility = Visibility.Collapsed;
            if (isReadMode)
            {
                RenderMarkdownToViewer();
                ReaderViewer.Visibility = Visibility.Visible;
                EditorTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ReaderViewer.Visibility = Visibility.Collapsed;
                EditorTextBox.Visibility = Visibility.Visible;
                EditorTextBox.Focus();
            }
        }

        // 更新列宽（确保单栏时占满窗口）
        UpdateColumnsLayout();

        // 同步 UI 状态
        if (ModeToggle != null) ModeToggle.IsChecked = isReadMode;
        if (ReadModeMenuItem != null) ReadModeMenuItem.IsChecked = isReadMode;
        if (EditModeMenuItem != null) EditModeMenuItem.IsChecked = !isReadMode;
        if (SplitToggle != null) SplitToggle.IsChecked = _isSplitPreview;
        if (SplitPreviewMenuItem != null) SplitPreviewMenuItem.IsChecked = _isSplitPreview;

        UpdateWindowTitle();
    }

    /// <summary>
    /// 设置是否启用分屏预览
    /// </summary>
    private void SetSplitPreview(bool enable)
    {
        _isSplitPreview = enable;
        // 重新按当前模式刷新布局
        SetMode(_isReadMode);
    }

    /// <summary>
    /// 将编辑器中的 Markdown 文本渲染为 FlowDocument 进行预览
    /// </summary>
    private void RenderMarkdownToViewer()
    {
        var markdown = EditorTextBox.Text ?? string.Empty;
        FlowDocument doc = Markdig.Wpf.Markdown.ToFlowDocument(markdown, _pipeline);
        doc.PagePadding = new Thickness(16);
        doc.FontSize = 14;
        ReaderViewer.Document = doc;
    }

    /// <summary>
    /// 更新窗口标题
    /// </summary>
    private void UpdateWindowTitle()
    {
        string untitled = GetString("Title_Untitled");
        string modeEdit = GetString("Title_Mode_Edit");
        string modeRead = GetString("Title_Mode_Read");

        string name = string.IsNullOrEmpty(_currentFilePath) ? untitled : System.IO.Path.GetFileName(_currentFilePath);
        string mode = _isReadMode ? modeRead : modeEdit;
        string dirty = _isDirty ? "*" : string.Empty;
        Title = $"Markdown_Z - {name} [{mode}]{dirty}";
    }

    /// <summary>
    /// 载入示例文本
    /// </summary>
    private void LoadSample()
    {
        if (!string.IsNullOrWhiteSpace(EditorTextBox.Text)) return;

        EditorTextBox.Text =
            "# 欢迎使用 Markdown_Z / Welcome\n\n" +
            "- 使用菜单或工具条切换 编辑/阅读 模式 / Toggle edit/read\n" +
            "- 支持 打开/保存/另存为 / Open/Save/Save As\n" +
            "- 快捷键：Ctrl+N/Ctrl+O/Ctrl+S\n\n" +
            "## 代码示例 / Code\n\n" +
            "```csharp\nConsole.WriteLine(\"Hello, Markdown!\");\n```\n\n" +
            "> 提示：阅读模式下展示渲染后的文档 / Read mode shows rendered document.";

        _currentFilePath = null;
        _isDirty = true;
        UpdateWindowTitle();
    }

    /// <summary>
    /// 文本变化事件：标记已修改，阅读/分屏时刷新预览
    /// </summary>
    private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _isDirty = true;
        UpdateWindowTitle();
        if (_isReadMode || _isSplitPreview) RenderMarkdownToViewer();
    }

    // 菜单：新建
    private void NewFile_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        EditorTextBox.Clear();
        _currentFilePath = null;
        _isDirty = false;
        SetMode(isReadMode: false);
        LoadSample();
    }

    // 菜单：打开
    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        var dlg = new OpenFileDialog
        {
            Title = GetString("Dialog_Open_Title"),
            Filter = GetString("Dialog_Open_Filter")
        };
        if (dlg.ShowDialog() == true)
        {
            var text = File.ReadAllText(dlg.FileName, Encoding.UTF8);
            EditorTextBox.Text = text;
            _currentFilePath = dlg.FileName;
            _isDirty = false;
            SetMode(isReadMode: false); // 打开后默认进入编辑模式
        }
    }

    // 菜单：保存（无路径时转为“另存为”）
    private void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath)) { SaveFileAs_Click(sender, e); return; }
        WriteToFile(_currentFilePath);
    }

    // 菜单：另存为
    private void SaveFileAs_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = GetString("Dialog_Save_Title"),
            Filter = GetString("Dialog_Save_Filter"),
            FileName = string.IsNullOrEmpty(_currentFilePath) ? GetString("Title_Untitled") + ".md" : System.IO.Path.GetFileName(_currentFilePath)
        };
        if (dlg.ShowDialog() == true) WriteToFile(dlg.FileName);
    }

    // 文件：导出为 HTML（不含代码高亮资源）
    private void ExportHtml_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = GetString("Dialog_ExportHtml_Title"),
            Filter = GetString("Dialog_ExportHtml_Filter"),
            FileName = (string.IsNullOrEmpty(_currentFilePath) ? GetString("Title_Untitled") : System.IO.Path.GetFileNameWithoutExtension(_currentFilePath)) + ".html"
        };
        if (dlg.ShowDialog() == true)
        {
            var markdown = EditorTextBox.Text ?? string.Empty;
            string body = global::Markdig.Markdown.ToHtml(markdown, _pipeline);
            var title = string.IsNullOrEmpty(_currentFilePath) ? GetString("Export_Default_Title") : System.IO.Path.GetFileNameWithoutExtension(_currentFilePath);
            var html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"utf-8\">");
            html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            html.AppendLine($"  <title>{System.Net.WebUtility.HtmlEncode(title)}</title>");
            html.AppendLine("  <style>");
            html.AppendLine("body{font-family:-apple-system,Segoe UI,Roboto,Helvetica,Arial,\"Segoe UI Emoji\",\"Segoe UI Symbol\";line-height:1.6;padding:2rem;max-width:980px;margin:0 auto;color:#222;background:#fff;}\n" +
                            "pre,code{font-family:Consolas,Monaco,Menlo,monospace;} pre{background:#f6f8fa;padding:12px;border-radius:6px;overflow:auto;}\n" +
                            "h1,h2,h3{border-bottom:1px solid #eaecef;padding-bottom:.3em;} blockquote{color:#6a737d;border-left:4px solid #dfe2e5;padding:.5em 1em;margin:1em 0;}\n" +
                            "table{border-collapse:collapse} th,td{border:1px solid #dfe2e5;padding:6px 13px;} img{max-width:100%;}");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine(body);
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            File.WriteAllText(dlg.FileName, html.ToString(), new UTF8Encoding(false));
        }
    }

    /// <summary>
    /// 实际写文件逻辑
    /// </summary>
    private void WriteToFile(string filePath)
    {
        File.WriteAllText(filePath, EditorTextBox.Text ?? string.Empty, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        _currentFilePath = filePath;
        _isDirty = false;
        UpdateWindowTitle();
        if (_isReadMode || _isSplitPreview) RenderMarkdownToViewer();
    }

    // 菜单：退出应用
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        Close();
    }

    // 工具条：切换为阅读/编辑
    private void ModeToggle_Checked(object sender, RoutedEventArgs e) => SetMode(isReadMode: true);
    private void ModeToggle_Unchecked(object sender, RoutedEventArgs e) => SetMode(isReadMode: false);

    // 菜单：点击“编辑模式/阅读模式”
    private void EditModeMenuItem_Click(object sender, RoutedEventArgs e) => SetMode(isReadMode: false);
    private void ReadModeMenuItem_Click(object sender, RoutedEventArgs e) => SetMode(isReadMode: true);

    /// <summary>
    /// 若存在未保存修改，弹窗确认是否放弃
    /// </summary>
    private bool ConfirmDiscardChanges()
    {
        if (!_isDirty) return true;
        var result = MessageBox.Show(
            GetString("Dialog_DiscardChanges"),
            GetString("Dialog_Confirm_Title"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    // ===================== 语言切换相关 =====================
    /// <summary>
    /// 根据系统语言初始化界面语言（中文或英文）
    /// </summary>
    private void InitializeLanguageBySystem()
    {
        var ui = System.Globalization.CultureInfo.CurrentUICulture;
        var tag = ui.TwoLetterISOLanguageName.ToLowerInvariant() == "zh" ? "zh-CN" : "en-US";
        ApplyLanguage(tag);
    }

    /// <summary>
    /// 应用指定语言（替换资源字典）
    /// </summary>
    private void ApplyLanguage(string cultureTag)
    {
        var appRes = Application.Current?.Resources;
        if (appRes == null) return;
        var dicts = appRes.MergedDictionaries;
        var existing = dicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Resources/Strings."));
        var newDict = new ResourceDictionary { Source = new System.Uri($"Resources/Strings.{cultureTag}.xaml", System.UriKind.Relative) };
        if (existing != null)
        {
            int idx = dicts.IndexOf(existing);
            if (idx >= 0) dicts[idx] = newDict; else dicts.Add(newDict);
        }
        else
        {
            dicts.Add(newDict);
        }
        if (LangZhMenuItem != null) LangZhMenuItem.IsChecked = cultureTag.StartsWith("zh");
        if (LangEnMenuItem != null) LangEnMenuItem.IsChecked = cultureTag.StartsWith("en");
        UpdateWindowTitle();
    }

    /// <summary>
    /// 获取资源字典中的字符串（若不存在则返回 key 本身）
    /// </summary>
    private string GetString(string key)
    {
        var obj = TryFindResource(key) ?? Application.Current?.TryFindResource(key);
        return obj as string ?? key;
    }

    // 语言菜单事件
    private void LangZhMenuItem_Click(object sender, RoutedEventArgs e) => ApplyLanguage("zh-CN");
    private void LangEnMenuItem_Click(object sender, RoutedEventArgs e) => ApplyLanguage("en-US");

    // 分屏菜单与工具条按钮事件
    private void SplitPreviewMenuItem_Click(object sender, RoutedEventArgs e) => SetSplitPreview(SplitPreviewMenuItem.IsChecked);
    private void SplitToggle_Checked(object sender, RoutedEventArgs e) => SetSplitPreview(true);
    private void SplitToggle_Unchecked(object sender, RoutedEventArgs e) => SetSplitPreview(false);

    /// <summary>
    /// 根据当前模式/是否分屏，调整 Grid 列宽，让单栏时占满窗口
    /// </summary>
    private void UpdateColumnsLayout()
    {
        if (_isSplitPreview)
        {
            // 分屏：编辑区与阅读区各占一半，分隔条宽 5px
            EditorColumn.Width = new GridLength(1, GridUnitType.Star);
            SplitterColumn.Width = new GridLength(5);
            ReaderColumn.Width = new GridLength(1, GridUnitType.Star);

            Grid.SetColumn(EditorTextBox, 0);
            Grid.SetColumnSpan(EditorTextBox, 1);
            Grid.SetColumn(ReaderViewer, 2);
            Grid.SetColumnSpan(ReaderViewer, 1);
        }
        else
        {
            // 单栏：隐藏分隔条列
            SplitterColumn.Width = new GridLength(0);

            if (_isReadMode)
            {
                // 阅读模式：阅读区独占全宽
                EditorColumn.Width = new GridLength(0);
                ReaderColumn.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(ReaderViewer, 2);
                Grid.SetColumnSpan(ReaderViewer, 1);
            }
            else
            {
                // 编辑模式：编辑区独占全宽
                ReaderColumn.Width = new GridLength(0);
                EditorColumn.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(EditorTextBox, 0);
                Grid.SetColumnSpan(EditorTextBox, 1);
            }
        }
    }
}
