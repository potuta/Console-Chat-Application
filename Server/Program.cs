using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        private static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> clientList = new List<Socket>();
        private static byte[] buffer = new byte[1024];
        private static string remoteAddress;

        static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();
            for (int i = 0; i < clientList.Count; i++)
            {
                while (clientList[i].Connected)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        Console.Write("You: ");
                        string sendText = Console.ReadLine();
                        byte[] sendData = Encoding.ASCII.GetBytes(sendText);
                        clientList[i].BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(BeginSendData), clientList[i]);
                    }
                }
            }
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            server.Bind(new IPEndPoint(IPAddress.Any, 16969));
            server.Listen(5);
            server.BeginAccept(new AsyncCallback(BeginAcceptClient), null);
        }

        private static void BeginAcceptClient(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client = server.EndAccept(ar);
                clientList.Add(client);
                remoteAddress = client.RemoteEndPoint.ToString();
                Console.WriteLine(remoteAddress + " Client Connected");
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceiveData), client);
                server.BeginAccept(new AsyncCallback(BeginAcceptClient), null);
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void BeginReceiveData(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            for (int i = 0; i < clientList.Count; i++)
            {
                while (clientList[i].Connected)
                {
                    client = clientList[i];
                    int receivedData = client.EndReceive(ar);
                    byte[] dataBuff = new byte[receivedData];
                    Array.Copy(buffer, dataBuff, receivedData);
                    remoteAddress = client.RemoteEndPoint.ToString();
                    string receivedText = Encoding.ASCII.GetString(dataBuff);
                    Console.WriteLine(remoteAddress + ": " + receivedText);
                    client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceiveData), client);
                    break;
                }
            }
        }

        private static void BeginSendData(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            for (int i = 0; i < clientList.Count; i++)
            {
                while (clientList[i].Connected)
                {
                    client = clientList[i];
                    client.EndSend(ar);
                    string sendText = Console.ReadLine();
                    byte[] sendData = Encoding.ASCII.GetBytes(sendText);
                    Console.Write("You: ");
                    client.BeginSend(sendData, 0 , sendData.Length, SocketFlags.None, new AsyncCallback(BeginSendData), client);
                    break;
                }
            }
        }
    }
}