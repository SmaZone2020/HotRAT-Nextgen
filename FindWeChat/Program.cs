using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

class GetWeChatInfo
{
    const int PROCESS_WM_READ = 0x0010;

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    class Data
    {
        public string nick { get; set; }
        public string name { get; set; }
        public string phone {  get; set; }
        public string tips { get;set; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine(await Get());
        Console.ReadKey();
    }
    static async Task<string> Get()
    {
        try
        {
            string processName = "WeChat";
            string moduleName = "WeChatWin.dll";

            Console.WriteLine("从数据库获取最新偏移值...");

            string url = "http://raw.githubusercontent.com/SmaZone2020/WeChatOffsetLib/main/new.json";

            HttpClient client = new HttpClient();
            var jsonInfo = await client.GetStringAsync(url);
            Console.WriteLine($"GET {url}\n{jsonInfo}");

            Data data = JsonConvert.DeserializeObject<Data>(jsonInfo);

            int phoneOffset = Convert.ToInt32(data.phone.Substring(2), 16);
            int nameOffset = Convert.ToInt32(data.name.Substring(2), 16);
            int nickOffset = Convert.ToInt32(data.nick.Substring(2), 16);

            int processId = GetProcessIdByName(processName);
            if (processId == 0)
            {
                Console.WriteLine();
                return "未找到微信";
            }

            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine();
                return "无法打开微信";
            }

            IntPtr moduleBaseAddress = GetModuleBaseAddress(processId, moduleName);
            if (moduleBaseAddress == IntPtr.Zero)
            {
                Console.WriteLine();
                CloseHandle(processHandle);
                return "未找到模块地址";
            }
            IntPtr targetAddress = IntPtr.Add(moduleBaseAddress, nameOffset);
            string result = ReadStringFromMemory(processHandle, targetAddress, 100);
            return result;
        }
        catch(Exception ex)
        {
            return ex.Message;
        }
    }

    private static int GetProcessIdByName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length > 0)
        {
            return processes[0].Id;
        }
        return 0;
    }

    private static IntPtr GetModuleBaseAddress(int processId, string moduleName)
    {
        Process process = Process.GetProcessById(processId);
        foreach (ProcessModule module in process.Modules)
        {
            if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                return module.BaseAddress;
            }
        }
        return IntPtr.Zero;
    }

    private static string ReadStringFromMemory(IntPtr processHandle, IntPtr address, int size)
    {
        byte[] buffer = new byte[size];
        if (ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesRead) && bytesRead > 0)
        {
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }
        return string.Empty;
    }
}
