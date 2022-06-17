using System.Text;
using System.Net.Sockets;

namespace LB3
{
    class Chat
    {
        const string ExitCommand = "#Exit";
        const string Arrow = "----------> ";
        const string ClientEnter = "ENTERED IN THE CHAT!";
        const string ClientExit = " EXIT FROM CHAT!";
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
            TcpClient NewClient;
            bool IsExit;

            while (IsMessaging)
            {
                ClientData = UnformatMessage(UDP1.ReceiveBroadcast(), out IsExit).Split(' ');
                if (ClientData[2] != UserIP)
                {
                    NewClient = TCP1.AddClient(ClientData[2], UserIP);
                    Task.Run(() => BeginListenClient(NewClient));
                    Console.WriteLine($"{ClientData[0]} {ClientData[1]}[{ClientData[2]}] {ClientData[3]} {ClientData[4]}" +
                    $" {ClientData[5]} {ClientData[6]}");
                }
            }
        }
        void BeginAddClients()
        {
            TcpClient Newclient;

            TCP1.RunServer(true);
            while (IsMessaging)
            {
                Newclient = TCP1.AddClient(null, UserIP);
                Task.Run(() => BeginListenClient(Newclient));
            }
        }
        void BeginMessaging()
        {
            string Message;

            do
            {
                Message = Console.ReadLine();
                if (Message == ExitCommand)
                {
                    TCP1.SendMessage(FormatMessage($"{UserName}[{UserIP}]", 2));
                    IsMessaging = false;
                    TCP1.Disconnect();
                    TCP1.RunServer(false);
                    UDP1.Receiver.Close();
                }
                else { TCP1.SendMessage(FormatMessage($"{UserName}[{UserIP}]: {Message}", 0)); }
            } while (IsMessaging);
        }
        void BeginListenClient(TcpClient Client)
        {
            byte[] Message = new byte[512];
            bool IsExit;
            Console.WriteLine("Listening");

            while (IsMessaging)
            {
                Client.Client.Receive(Message);
                Console.WriteLine(UnformatMessage(Message, out IsExit));
                if (IsExit)
                {
                    Console.WriteLine("Removed");
                    TCP1.RemoveClient(Client);
                    return;
                }
            }
        }
        byte[] FormatMessage(string Message, byte Type)
        {
            byte[] FormatedMessage = new byte[512];
            byte[] MessageToSend = new byte[256];

            switch (Type)
            {
                case 0:
                    {
                        MessageToSend = Encoding.UTF8.GetBytes(Message);
                        break;
                    }
                case 1:
                    {
                        MessageToSend = Encoding.UTF8.GetBytes(Message + ClientEnter);
                        break;
                    }
                case 2:
                    {
                        MessageToSend = Encoding.UTF8.GetBytes(Message + ClientExit);
                        break;
                    }
            }
            FormatedMessage[0] = Type;
            FormatedMessage[1] = (byte)MessageToSend.Length;
            MessageToSend.CopyTo(FormatedMessage, 2);

            return FormatedMessage;
        }
        string UnformatMessage(byte[] Message, out bool IsDisconnect)
        {
            if (Message[0] == 2) { IsDisconnect = true; }
            else { IsDisconnect = false; }

            if (Message[0] != 0) return Arrow + Encoding.UTF8.GetString(Message, 2, Message[1]);
            else return Encoding.UTF8.GetString(Message, 2, Message[1]);
        }
        public void RunChat()
        {
            try
            {
                IdentifyUser();
                TCP1 = new(UserIP);
                UDP1 = new(UserIP);
                UDP1.SendBroadcast(FormatMessage($"{UserName} {UserIP} ", 1));
                Task[] Tasks = new Task[3];
                Tasks[0] = Task.Run(() => BeginAddClients());
                Tasks[1] = Task.Run(() => BeginReceiveBroadcast());
                Tasks[2] = Task.Run(() => BeginMessaging());
                Console.WriteLine(Arrow + "CHAT STARTED!");
                Task.WaitAll(Tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}