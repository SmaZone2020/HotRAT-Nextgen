using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRAT;

namespace Data
{

    public static class MainData
    {
        public static ObservableCollection<Device> DeviceL { get; set; } = new ObservableCollection<Device>();
        public static string ImgBase64 = ""; 
        public static async void DeleteDevice(string ip)
        {
            var deviceToRemove = MainData.DeviceL.FirstOrDefault(d => d.IP == ip);
            if (deviceToRemove != null)
            {

                if (MainData.SelectDevice == deviceToRemove) 
                {
                    await Program.server.DisconnectClientByIPAsync(deviceToRemove.IP);
                    MainData.DeviceL.Remove(deviceToRemove);
                    MainData.SelectDevice = null;
                }
                
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
