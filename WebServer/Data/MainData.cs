using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{

    public static class MainData
    {
        public static ObservableCollection<Device> DeviceL { get; set; } = new ObservableCollection<Device> { };
        public static void DeleteDevice(string ip)
        {
            var deviceToRemove = MainData.DeviceL.FirstOrDefault(d => d.IP == ip);
            if (deviceToRemove != null)
            {
                if (MainData.SelectDevice == deviceToRemove) MainData.SelectDevice = null;
                MainData.DeviceL.Remove(deviceToRemove);
            }
        }

        public static string BasePath { get; set; } = "Config.ncfg";
        public static long DeviceNumber { get; set; }
        public static string StartTime { get; set; }
        public static Device SelectDevice { get; set; }
        public static string NowDeviceName { get; set; } = Environment.UserName;
    }

    public static class ServerSettings
    {
        public static short Port { get; set; } = 2026;
    }
}
