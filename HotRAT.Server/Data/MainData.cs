using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace Data
{
    public static class MainData
    {
        public static string BasePath { get; set; } = "Config.ncfg";
        public static long DeviceNumber { get; set; }
        public static string StartTime { get; set; }
        public static Device SelectDevice { get; set; }
        public static string NowDeviceName { get; set; } = Environment.UserName;
    }
    public static class SoftWareSettings
    {
        public static bool IsLight { get; set; } = true;
        public static object DarkBackColor { get; set; } = ColorConverter.ConvertFromString("#FF232323");
        public static object LightBackColor { get; set; } = ColorConverter.ConvertFromString("#FFF1F1F1");
        public static Brush DarkFontColor { get; set; } = Brushes.White;
        public static Brush LightFontColor { get; set; } = Brushes.Black;
    }
    public static class ServerSettings
    {
        public static short Port { get; set; } = 2026;
    }
}
