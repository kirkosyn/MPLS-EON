using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CableCloud
{
    class NodeCloud
    {
        Socket mySocket;

        EndPoint endRemote;
        byte[] buffer;
        int id;
        public NodeCloud(int id)
        {
            this.id = id;
            Switch.nodeCollection.Add(new ObservableCollection<string>());
        }

        public void CreateSocket(string IP, int port)
        {
            string myIp;
            int myport;
            myIp = IP;
            myport = port;

            IPAddress ipAddress = IPAddress.Parse(myIp);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, myport);

            mySocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            mySocket.Bind(localEndPoint);
        }
        public void Connect(string IP, int port)
        {
            string toIp = IP;
            int toPort;
            toPort = port;

            IPAddress ipAddress = IPAddress.Parse(toIp);

            endRemote = new IPEndPoint(ipAddress, toPort);
            //mySocket.Bind(endRemote);
            mySocket.Connect(endRemote);
            buffer = new byte[1024];

            mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
                new AsyncCallback(MessageCallback), buffer);
        }
        private void MessageCallback(IAsyncResult result)
        {

            try
            {

                byte[] receivedData = new byte[1024];
                receivedData = (byte[])result.AsyncState;

                ASCIIEncoding encoding = new ASCIIEncoding();

                int i = receivedData.Length - 1;
                while (receivedData[i] == 0)
                    --i;

                byte[] auxtrim = new byte[i + 1];
                Array.Copy(receivedData, auxtrim, i + 1);

                string receivedMessage = encoding.GetString(auxtrim);

                Console.WriteLine();
                Console.Write(this.GetTimestamp() + " : ");

                int index = receivedMessage.IndexOf("<port>");
                string msg = receivedMessage.Substring(0, index);
                Console.WriteLine("Odebrana została od węzła wiadomość o treści: " + msg);

                //Console.WriteLine("Otrzymałem wiadomość od węzła\n" + receivedMessage);

                //tu switchujemy to co przechodzi
                Switch.SwitchBufer(receivedMessage);


                buffer = new byte[1024];
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
                    new AsyncCallback(MessageCallback), buffer);

            }
            catch (Exception ex)
            {

            }
        }
        public void Send(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {
            lock (Switch.nodeCollection.ElementAt(id - 1))
            {
                string s = Switch.nodeCollection.ElementAt(id - 1).Last();
                //Console.WriteLine("Wysyłam wiadomość do węzła");
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(s);

                mySocket.Send(sending);

                Console.Write(this.GetTimestamp() + " : ");
                int index = s.IndexOf("<port>");
                string msg = s.Substring(0, index);
                Console.WriteLine("Wysłana została do węzła wiadomość o treści: " + msg);

            }
        }
        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }

        public void SendThread()
        {
            lock (Switch.nodeCollection.ElementAt(id - 1))
            {
                Switch.nodeCollection.ElementAt(id - 1).CollectionChanged += Send;
            }
        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}









