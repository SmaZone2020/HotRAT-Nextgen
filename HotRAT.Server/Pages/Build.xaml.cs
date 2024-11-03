using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace HotRAT.Server.Pages
{
    /// <summary>
    /// Build.xaml 的交互逻辑
    /// </summary>
    public partial class Build : Page
    {
        public Build()
        {
            InitializeComponent();
        }

        private async void build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "Client\\Template.dll";
                string csPath = "Client\\Program.cs";
                File.Copy(path, csPath);
                var content = File.ReadAllText(path);
                File.WriteAllText(csPath, content.Replace("{IPADDRESS}", ip.Text).Replace("{PORT}", port.Text));
                string exePath = Path.Combine(Directory.GetCurrentDirectory(), "Client", "dotnet.exe");
                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = "build";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                string batFilePath = Path.Combine(Directory.GetCurrentDirectory(), "build.bat");
                string batContent = "@echo off\r\ncd Client\r\ndotnet.exe publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true\r\ndel /Q Program.cs\r\nstart \"\" \"%cd%\\bin\\Release\\net6.0\\win-x64\\publish\"\r\ndel /Q build.bat";
                File.WriteAllText(batFilePath, batContent);

                await RunBatchFileAsync(batFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task RunBatchFileAsync(string batFilePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batFilePath}\"",  // 使用 /c 参数运行并关闭
                    CreateNoWindow = true,                // 不显示窗口
                    UseShellExecute = false,              // 必须为 false 以重定向输出
                    RedirectStandardOutput = true,        // 重定向标准输出
                    RedirectStandardError = true,         // 重定向标准错误
                    RedirectStandardInput = false,        // 不需要输入
                    StandardOutputEncoding = Encoding.UTF8,  // 设置输出编码
                    StandardErrorEncoding = Encoding.UTF8,   // 设置错误编码
                },
                EnableRaisingEvents = true
            };
            process.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en";
            process.OutputDataReceived += (sender, args) =>
            {
                AppendToLog(args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                AppendToLog(args.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        private void AppendToLog(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                string utf16Message = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(message));
                Dispatcher.Invoke(() =>
                {
                    build_log.Text += utf16Message + Environment.NewLine + Environment.NewLine;
                    scrollViewer.ScrollToEnd();
                });
            }
        }

    }
}
