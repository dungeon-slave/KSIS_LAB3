using System.Text;
using System.Net.Sockets;

namespace LB3
{
    class Chat
    {
        readonly string[] StringElements = { "#Exit", "----------> ", "ENTERED IN THE CHAT!", " EXIT FROM CHAT!", "CHAT STARTED!" };
        string? UserIP;
        string? UserName;
        UDP? UDP1;
        TCP? TCP1;
        bool IsMessaging = true;

        void IdentifyUser()
        {
            Console.Write("Input Name: ");
            UserName = Console.ReadLine();
            Console.Write("Input IP: ");
            UserIP = Console.ReadLine().Trim();
            Console.WriteLine();
        }
        void BeginReceiveBroadcast()
        {
            string[] ClientData;
            bool IsExit;

            try
            {
                while (IsMessaging)
                {
                    ClientData = UnformatMessage(UDP1.ReceiveBroadcast(), out IsExit).Split(' ');
                    if (ClientData[2] != UserIP)
                    {
                        TcpClient NewClient = TCP1.AddClient(ClientData[2]);
                        Task.Run(() => BeginListenClient(NewClient));
                        Console.WriteLine($"{ClientData[0]} {ClientData[1]}[{ClientData[2]}] {ClientData[3]} {ClientData[4]}" +
                        $" {ClientData[5]} {ClientData[6]}");
                    }
                }
            }
            catch (System.Exception)
            {
                return;
            }
        }
        void BeginAddClients()
        {
            try
            {
                TCP1.RunServer(true);
                while (IsMessaging)
                {
                    TcpClient NewClient = TCP1.AddClient(null);
                    Task.Run(() => BeginListenClient(NewClient));
                }
                TCP1.RunServer(false);                
            }
            catch (System.Exception)
            {
                return;
            }
        }
        void BeginMessaging()
        {
            string Message;

            try
            {
                do
                {
                    Message = Console.ReadLine();
                    if (Message == StringElements[0])
                    {
                        IsMessaging = false;
                        TCP1.SendMessage(FormatMessage($"{UserName}[{UserIP}]", 2));
                        TCP1.Disconnect();
                    }
                    else { TCP1.SendMessage(FormatMessage($"{UserName}[{UserIP}]: {Message}", 0)); }
                } while (IsMessaging);
            }
            catch (System.Exception)
            {
                return;
            }
        }
        void BeginListenClient(TcpClient Client)
        {
            byte[] Message = new byte[512];
            bool IsExit;

            try
            {
                while (IsMessaging)
                {
                    Client.Client.Receive(Message);
                    Console.WriteLine(UnformatMessage(Message, out IsExit));
                    if (IsExit)
                    {
                        TCP1.RemoveClient(Client);
                        return;
                    }                        
                }
            }
            catch (System.Exception)
            {
                return;
            }
        }
        byte[] FormatMessage(string Message, byte Type)
        {
            byte[] FormatedMessage = new byte[512];

            switch (Type)
            {
                case 1:
                    {
                        Message = StringElements[1] + Message + StringElements[2];
                        break;
                    }
                case 2:
                    {
                        Message = StringElements[1] + Message + StringElements[3];
                        break;
                    }
            }
            FormatedMessage[0] = Type;
            FormatedMessage[1] = (byte)Encoding.UTF8.GetBytes(Message).Length;
            Encoding.UTF8.GetBytes(Message).CopyTo(FormatedMessage, 2);

            return FormatedMessage;
        }
        string UnformatMessage(byte[] Message, out bool IsDisconnect)
        {
            if (Message[0] == 2) { IsDisconnect = true; }
            else { IsDisconnect = false; }

            return Encoding.UTF8.GetString(Message, 2, Message[1]);
        }
        public void RunChat()
        {
            try
            {
                IdentifyUser();
                TCP1 = new(UserIP);
                UDP1 = new(UserIP);
                UDP1.SendBroadcast(FormatMessage($"{UserName} {UserIP} ", 1));
                Task[] Tasks = {
                    Task.Run(() => BeginAddClients()),
                    Task.Run(() => BeginReceiveBroadcast()),
                    Task.Run(() => BeginMessaging())
                };
                Console.WriteLine(StringElements[1] + StringElements[4]);
                Task.WaitAny(Tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
