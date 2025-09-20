using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Markdown_Z.Helpers;

internal static class FileAssociation
{
    private const string ProgId = "Markdown_Z.md"; // Our ProgID
    private const string AppRegRoot = "Software\\Markdown_Z"; // Per-user app root

    public static void EnsureRegisteredAndPromptIfNotDefault()
    {
        try
        {
            RegisterProgId();
            RegisterOpenWith();
            RegisterCapabilities();

            if (!IsDefaultForMd())
            {
                // One-time prompt to guide user to Default Apps
                if (!GetAskedFlag())
                {
                    SetAskedFlag();
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var result = System.Windows.MessageBox.Show(
                            "是否打开系统‘默认应用’设置，将 Markdown_Z 设为 .md 默认打开方式？",
                            "Markdown_Z",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Question);
                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            TryLaunchDefaultAppsUi();
                        }
                    });
                }
            }
        }
        catch
        {
            // Never crash app due to registration issues
        }
    }

    private static void RegisterProgId()
    {
        string? exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe)) return;

        using var progIdKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{ProgId}");
        progIdKey?.SetValue(null, "Markdown_Z Markdown Document");

        using var iconKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{ProgId}\\DefaultIcon");
        iconKey?.SetValue(null, $"\"{exe}\",0");

        using var cmdKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{ProgId}\\shell\\open\\command");
        cmdKey?.SetValue(null, $"\"{exe}\" \"%1\"");

        // Also register under Applications to show in ‘打开方式’列表
        using var appCmdKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\Applications\\{Path.GetFileName(exe)}\\shell\\open\\command");
        appCmdKey?.SetValue(null, $"\"{exe}\" \"%1\"");
    }

    private static void RegisterOpenWith()
    {
        using var k = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.md\\OpenWithProgids");
        try
        {
            // Prefer REG_NONE with zero-length data; fall back to empty string if not supported
            k?.SetValue(ProgId, Array.Empty<byte>(), RegistryValueKind.None);
        }
        catch
        {
            k?.SetValue(ProgId, "");
        }
    }

    private static void RegisterCapabilities()
    {
        string? exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe)) return;

        using var caps = Registry.CurrentUser.CreateSubKey($"{AppRegRoot}\\Capabilities");
        caps?.SetValue("ApplicationName", "Markdown_Z");
        caps?.SetValue("ApplicationDescription", ".md 文件查看与编辑");
        caps?.SetValue("ApplicationIcon", $"\"{exe}\",0");

        using var fa = Registry.CurrentUser.CreateSubKey($"{AppRegRoot}\\Capabilities\\FileAssociations");
        fa?.SetValue(".md", ProgId);

        // Make Windows list our app on Default Apps page (per-user)
        using var regApps = Registry.CurrentUser.CreateSubKey("Software\\RegisteredApplications");
        regApps?.SetValue("Markdown_Z", $"{AppRegRoot}\\Capabilities");
    }

    private static bool IsDefaultForMd()
    {
        try
        {
            using var uc = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.md\\UserChoice");
            var progId = uc?.GetValue("ProgId") as string;
            if (!string.IsNullOrWhiteSpace(progId))
                return string.Equals(progId, ProgId, StringComparison.OrdinalIgnoreCase);

            // Fallback when no explicit UserChoice exists
            using var dot = Registry.CurrentUser.OpenSubKey("Software\\Classes\\.md");
            var def = dot?.GetValue(null) as string; // default value
            return string.Equals(def, ProgId, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static void TryLaunchDefaultAppsUi()
    {
        // Direct to .md mapping (Win10/11)
        if (TryShellOpen("ms-settings:defaultapps?filetype=.md")) return;

        // App's page (Win11 22H2+)
        if (TryShellOpen("ms-settings:defaultapps?name=Markdown_Z")) return;

        // By-filetype tab
        if (TryShellOpen("ms-settings:defaultapps?activetab=byfiletype")) return;

        // Classic Advanced Associations UI
        if (TryLaunchAdvancedAssociationUi()) return;

        // Legacy Control Panel
        TryShellOpen("control.exe", "/name Microsoft.DefaultPrograms /page pageDefaultProgram");
    }

    private static bool TryShellOpen(string uri, string? args = null)
    {
        try
        {
            var psi = new ProcessStartInfo(uri)
            {
                UseShellExecute = true,
                Arguments = args ?? string.Empty
            };
            Process.Start(psi);
            return true;
        }
        catch { return false; }
    }

    [ComImport]
    [Guid("1F76A169-F994-40AC-8FC8-0959E8874710")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IApplicationAssociationRegistrationUI
    {
        [PreserveSig]
        int LaunchAdvancedAssociationUI([MarshalAs(UnmanagedType.LPWStr)] string appRegName);
    }

    [ComImport]
    [Guid("1968106D-F3B5-44CF-890E-116FCB9ECEF1")]
    private class ApplicationAssociationRegistrationUI { }

    private static bool TryLaunchAdvancedAssociationUi()
    {
        try
        {
            var ui = (IApplicationAssociationRegistrationUI)new ApplicationAssociationRegistrationUI();
            int hr = ui.LaunchAdvancedAssociationUI("Markdown_Z");
            return hr >= 0; // S_OK or success code
        }
        catch { return false; }
    }

    private static bool GetAskedFlag()
    {
        try
        {
            using var k = Registry.CurrentUser.CreateSubKey(AppRegRoot);
            var vObj = k?.GetValue("AskedDefault");
            return vObj is int i && i != 0;
        }
        catch { return false; }
    }

    private static void SetAskedFlag()
    {
        try
        {
            using var k = Registry.CurrentUser.CreateSubKey(AppRegRoot);
            k?.SetValue("AskedDefault", 1, RegistryValueKind.DWord);
        }
        catch { /* ignore */ }
    }
}
