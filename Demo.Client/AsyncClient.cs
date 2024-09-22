using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCPClient
{
    public TcpClient client;
    public string serverAddress;
    public int serverPort;
    public bool isSend = true;
    public bool IsConnected = false;
    public TCPClient(string address, int port)
    {
        this.serverAddress = address;
        this.serverPort = port;
    }

    public async Task ConnectAndSend(string message)
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverAddress, serverPort);
            await SendMessageAsync(message);
            await Handle.ReceiveMessagesAsync();
        }
        catch (Exception ex)
        {
            isSend = false;
            Console.WriteLine("Error connecting and sending: " + ex.Message);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message + "[A]");
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            isSend = false;
            Console.WriteLine("Error sending message: " + ex.Message);
        }
    }


}