using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace HotRAT.Server.Pages
{
    /// <summary>
    /// Log.xaml 的交互逻辑
    /// </summary>
    public partial class Log : Page
    {
        public static ObservableCollection<TextBlock> Logs { get; set; } = new ObservableCollection<TextBlock>();
        public Log()
        {
            InitializeComponent();
            LogBox.ItemsSource = Logs;
            Brush brush;
            if (SoftWareSettings.IsLight)
                brush = Brushes.Black;
            else
                brush = Brushes.White;
            AddLog("Thanks for using HotRAT Nextgen, which is an open source project", brush, false);
            AddLog("You are responsible for the consequences of anything you do with it", brush, false);
            AddLog("Please do not use it for anything that violates your local laws and regulations", brush, false);
        }

        public static void AddLog(string content, Brush brush, bool showTime = true, int fontSize = 17)
        {
            // 调度到 UI 线程执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // 如果日志列表长度超过80，删除第一个项
                    if (Logs.Count > 80)
                    {
                        Logs.RemoveAt(0);
                    }

                    // 创建新的日志项
                    TextBlock tb = new TextBlock();
                    tb.Text = content;
                    if (showTime)
                        tb.Text = $"[{DateTime.Now.ToString("MMdd-HH:mm:ss")}]-{content}";
                    tb.Foreground = brush;
                    tb.FontSize = fontSize;
                    Logs.Add(tb);
                }
                catch { }
            });
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (LogBox.SelectedItem == null) return;
            var now = (TextBlock)LogBox.SelectedItem;
            Clipboard.SetText(now.Text);
        }
    }
}
