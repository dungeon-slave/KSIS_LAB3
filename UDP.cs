using System.Net;
using System.Net.Sockets;

namespace LB3
{
    class UDP
    {
        int UdpPort = 8888;
        public UdpClient Receiver;

        public UDP(string UserIP)
        {
            Receiver = new();
            Receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Receiver.Client.Bind(new IPEndPoint(IPAddress.Any, UdpPort));
        }
        public void SendBroadcast(byte[] UserData)
        {
            UdpClient Sender = new();

            Sender.Send(UserData, UserData.Length, "255.255.255.255", UdpPort);
            Sender.Dispose();
        }
        public byte[] ReceiveBroadcast()
        {
            byte[] Data = new byte[512];

            Receiver.Client.Receive(Data);

            return Data;
        }
    }
}
