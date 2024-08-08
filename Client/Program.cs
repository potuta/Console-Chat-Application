using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {   
        private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] buffer = new byte[1024];
        private static string remoteAddress;

        static void Main(string[] args)
        {
            LoopConnect();
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceiveData), client);
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Console.WriteLine("You: ");
                string sendText = Console.ReadLine();
                byte[] sendData = Encoding.ASCII.GetBytes(sendText);
                client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(BeginSendData), client);
            }
        }

        private static void BeginReceiveData(IAsyncResult ar)
        {

            Socket client = (Socket)ar.AsyncState;
            int receivedData = client.EndReceive(ar);
            byte[] dataBuff = new byte[receivedData];
            Array.Copy(buffer, dataBuff, receivedData);
            remoteAddress = client.RemoteEndPoint.ToString();
            string receivedText = Encoding.ASCII.GetString(dataBuff);
            Console.WriteLine(remoteAddress + ": " + receivedText);
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceiveData), client);
        }

        private static void BeginSendData(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSend(ar);
            string sendText = Console.ReadLine();
            byte[] sendData = Encoding.ASCII.GetBytes(sendText);
            Console.WriteLine("You: ");
            client.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(BeginSendData), client);
        }

        private static void LoopConnect()
        {
            int attempts = 0;
            while (!client.Connected)
            {
                try
                {
                    attempts++;
                    client.Connect(IPAddress.Loopback, 16969);
                }
                catch (SocketException ex)
                {
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: "+attempts);
                }
            }
            Console.WriteLine("Connected to server " + client.RemoteEndPoint);
        }
    }
}