using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class GetWeChatInfo
{
    const int PROCESS_WM_READ = 0x0010;

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);


    public static async Task<string> Get()
    {
        string processName = "WeChat";
        string moduleName = "WeChatWin.dll";
        string url = "https://raw.githubusercontent.com/SmaZone2020/WeChatOffsetLib/main/new.json"; // 获取微信偏移信息

        Dictionary<string, string> offsets = await FetchOffsetsAsync(url);
        if (offsets == null)
        {
            offsets = JsonConvert.DeserializeObject<Dictionary<string, string>>(@"
{
        ""phone"":""0x597CE28"",
        ""name"":""0x597CE08"",
        ""nick"":""0x597CEE8""
}
");
        }

        int processId = GetProcessIdByName(processName);
        if (processId == 0)
        {
            return "未找到微信";
        }

        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, processId);
        if (processHandle == IntPtr.Zero)
        {
            return "无法打开微信";
        }

        IntPtr moduleBaseAddress = GetModuleBaseAddress(processId, moduleName);
        if (moduleBaseAddress == IntPtr.Zero)
        {
            CloseHandle(processHandle);
        }

        string c_result = "";
        foreach (var offset in offsets)
        {
            int intOffset = ConvertHexStringToInt(offset.Value);
            IntPtr targetAddress = IntPtr.Add(moduleBaseAddress, intOffset);
            string result = ReadStringFromMemory(processHandle, targetAddress, 100);
            if(offset.Key == "phone")
            {
                c_result = $"{result.Split('\0')[0]}";
            }
        }

        CloseHandle(processHandle);
        return c_result;
    }
    private static async Task<Dictionary<string, string>> FetchOffsetsAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string json = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载或解析JSON失败: {ex.Message}");
                return null;
            }
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

    private static int ConvertHexStringToInt(string hexString)
    {
        if (hexString.StartsWith("0x"))
        {
            hexString = hexString.Substring(2);
        }
        return Convert.ToInt32(hexString, 16);
    }

}
