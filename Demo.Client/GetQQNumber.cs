using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class GetQQNumber
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);
    public static string Get()
    {
        IntPtr hWnd; StringBuilder className = new StringBuilder(64); string qqclassName;
        string res = "";
        for (int i = 65535; i <= 10000000; i++)
        {
            hWnd = (IntPtr)i;
            if (IsWindow(hWnd) && !IsWindowVisible(hWnd))
            {
                className.Length = 0;
                if (GetClassName(hWnd, className, className.Capacity) != 0)
                {
                    qqclassName = className.ToString();                  
                    if (qqclassName.StartsWith("NTQQOpenSdk") || qqclassName.StartsWith("OPENSDK_SHARE2QQ_QQ_WINCLASS"))
                    {
                        //Console.WriteLine($"窗口句柄：{hWnd.ToInt64()} = 窗口类名：{qqclassName}");
                        var match = Regex.Match(qqclassName, @"_(\d+)$");
                        if (match.Success)
                        {
                            res = match.Groups[1].Value; Console.WriteLine($"{res}");
                        }
                    }
                }
            }
        }
        return res;
    }
}