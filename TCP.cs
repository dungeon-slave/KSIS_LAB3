using System.Net;
using System.Net.Sockets;

namespace LB3
{
    class TCP
    {
        int TcpPort = 8889;
        List<TcpClient> Clients = new();
        TcpListener Server;

        public TCP(string UserIP)
        {
            this.Server = new(IPAddress.Parse(UserIP), TcpPort);
        }
        public void RunServer(bool IsRun)
        {
            if (IsRun)
            {
                Server.Start();
            }
            else
            {
                Server.Stop();
                Server.Server.Close();
            }
        }
        public TcpClient? AddClient(string? ClientIP)
        {
            TcpClient NewClient;

            if (ClientIP == null) { NewClient = Server.AcceptTcpClient(); }
            else
            {
                NewClient = new();
                NewClient.Connect(IPAddress.Parse(ClientIP), TcpPort);
            }
            Clients.Add(NewClient);

            return NewClient;
        }
        public void RemoveClient(TcpClient Client) { Clients.Remove(Client); }
        public void SendMessage(byte[] Message)
        {
            if (Clients != null)
            {
                foreach (var Client in Clients) { Client.Client.Send(Message); }
            }
        }
        public void Disconnect()
        {
            foreach (var Client in Clients)
            {
                Client.Close();
            }
        }
    }
}
