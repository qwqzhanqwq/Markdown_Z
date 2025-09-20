using System;
using System.Linq;
using System.Windows;
using Markdown_Z.Helpers;

namespace Markdown_Z;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 应用启动事件：创建主窗口并处理命令行参数（支持双击 .md 文件打开）
    /// </summary>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // 当主窗口关闭时退出应用
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        // 创建主窗口
        var main = new MainWindow();

        // 注册文件关联；必要时引导用户到“默认应用”设置
        FileAssociation.EnsureRegisteredAndPromptIfNotDefault();

        // 若有命令行参数，尝试将第一个参数作为要打开的 Markdown 文件
        if (e.Args is { Length: > 0 })
        {
            var firstArg = e.Args.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstArg))
            {
                try
                {
                    main.OpenFileByPath(firstArg!);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "无法打开文件：" + firstArg + "\n\n" + ex.Message,
                        "Markdown_Z",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // 显示主窗口
        MainWindow = main;
        main.Show();
    }
}
