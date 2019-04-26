using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    class Port
    {



        Socket mySocket;
        Socket listeningSocket;

        EndPoint endRemote, endLocal;
        byte[] buffer;

        public Port()
        {
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
        public void Connect()
        {
            mySocket.Listen(10);
            mySocket = mySocket.Accept();
            Console.WriteLine("Połączono z chmurą");
            //mySocket.Accept();
            //mySocket.BeginAccept(AcceptCallback, mySocket);
            buffer = new byte[1024];


            mySocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
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
                Console.WriteLine("Odebrana została od chmury wiadomość na porcie " + Label.GetPort(receivedMessage) + " o treści: " + msg);

                //Console.WriteLine("Otrzymana wiadomosc na porcie:" + Label.GetPort(receivedMessage) + "\n" + receivedMessage);

                //polecenia przesyslane do managera
                if (receivedMessage.Contains("connection:"))
                {
                    lock (SwitchingMatrix.agentCollection)
                    {
                        SwitchingMatrix.agentCollection.Add(receivedMessage);
                    }
                }
                else if (receivedMessage.Contains("delete:"))
                {
                    lock (SwitchingMatrix.agentCollection)
                    {
                        SwitchingMatrix.agentCollection.Add(receivedMessage);
                    }
                }
                //w przeciwnym przypadku wysyla do watki liczacego i ten nastepnie do wsysylajacego z powrotem do chmury
                else
                {
                    lock (SwitchingMatrix.computingCollection)
                    {
                        SwitchingMatrix.computingCollection.Add(receivedMessage);
                    }
                }


                buffer = new byte[1024];

                mySocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(MessageCallback), buffer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Send(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {

            // if (counter == 0)
            //{
            lock (SwitchingMatrix.sendCollection)
            {
                string s = SwitchingMatrix.sendCollection.Last();
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(s);
              //  Console.WriteLine(s);

                mySocket.Send(sending);
                Console.Write(this.GetTimestamp() + " : ");
                int index = s.IndexOf("<port>");
                string msg = s.Substring(0, index);
                Console.WriteLine("Wysłana została wiadomość o treści: " + msg);

            }
        }
        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }
        public void SendThread()
        {
            lock (SwitchingMatrix.sendCollection)
            {
                SwitchingMatrix.sendCollection.CollectionChanged += Send;
            }
        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}






