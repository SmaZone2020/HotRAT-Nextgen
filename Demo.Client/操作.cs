using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class 操作
{


    #region 按键操作
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    public const uint MOUSEEVENTF_LEFTUP = 0x04;
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
    public const uint MOUSEEVENTF_RIGHTUP = 0x10;
    public const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
    public const uint MOUSEEVENTF_MIDDLEUP = 0x40;

    public static void ClickMouse(string mouseChange)
    {
        uint downFlag = 0;
        uint upFlag = 0;

        switch (mouseChange.ToLower())
        {
            case "LeftDOWN":
                downFlag = MOUSEEVENTF_LEFTDOWN;
                break;
            case "LeftUP":
                upFlag = MOUSEEVENTF_LEFTUP;
                break;
            case "RightDOWN":
                downFlag = MOUSEEVENTF_RIGHTDOWN;
                break;
            case "RightUP":
                upFlag = MOUSEEVENTF_RIGHTUP;
                break;
            case "MiddleDOWN":
                downFlag = MOUSEEVENTF_MIDDLEDOWN;
                break;
            case "MiddleUP":
                upFlag = MOUSEEVENTF_MIDDLEUP;
                break;
            default:
                break;
        }

        if (downFlag != 0)
        {
            mouse_event(downFlag, 0, 0, 0, 0);
        }

        if (upFlag != 0)
        {
            mouse_event(upFlag, 0, 0, 0, 0);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    public const uint KEYEVENTF_KEYDOWN = 0x0000;
    public const uint KEYEVENTF_KEYUP = 0x0002;

    public static void SimulateKeyPress(int keyCode)
    {
        byte bVk = (byte)keyCode;
        keybd_event(bVk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        keybd_event(bVk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
    #endregion


    public static string ExecuteCommand(string command)
    {
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/C " + command;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();
        return output;
    }

    public static string GetFileList(string directoryPath)
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileSystemInfo[] fileSystemInfos = dirInfo.GetFileSystemInfos();
            string fileList = "";
            foreach (var fileInfo in fileSystemInfos)
            {
                string fileType = (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? "Dir" : "File";
                string creationTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
                string lastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                fileList += $"{fileInfo.Name}|{fileType}|{creationTime}|{lastWriteTime}\n";
            }
            return fileList;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
