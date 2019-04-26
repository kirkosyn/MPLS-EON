using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CableCloud
{
    class Agent
    {
        Socket mySocket;

        byte[] buffer;
        ObservableCollection<string> agentCollection;
        int id;

        public Agent(int id)
        {
            this.id = id;
            agentCollection = new ObservableCollection<string>();
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
            // CLI.ConnectedAgent();
            buffer = new byte[30240];

            mySocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
        new AsyncCallback(MessageCallback), buffer);
        }

        private void MessageCallback(IAsyncResult result)
        {
            try
            {
                byte[] receivedData = new byte[30240];
                receivedData = (byte[])result.AsyncState;

                ASCIIEncoding encoding = new ASCIIEncoding();

                int i = receivedData.Length - 1;
                while (receivedData[i] == 0)
                    --i;

                byte[] auxtrim = new byte[i + 1];
                Array.Copy(receivedData, auxtrim, i + 1);

                string receivedMessage = encoding.GetString(auxtrim);

                Console.Write(this.GetTimestamp() + " : ");
                if (receivedMessage.Contains("<cable_cloud"))
                {
                    Console.WriteLine("Odebrana została od agenta " + id + " informacja o konfiguracji łączy");
                }
                else
                    Console.WriteLine("Odebrana została od agenta " + id + " wiadomość o treści: " + receivedMessage);

                //Console.WriteLine("Od Agenta " + id + "  " + ":\n " + receivedMessage + "\n");
                lock (agentCollection)
                {
                    agentCollection.Add(receivedMessage);
                }
                buffer = new byte[30240];

                mySocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(MessageCallback), buffer);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Message callback execption:" + ex.ToString());
            }
        }


        public void Send(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {
            lock (agentCollection)
            {
                string s = agentCollection.Last();
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(s);

                mySocket.Send(sending);
                Console.Write(this.GetTimestamp() + " : ");
                Console.WriteLine("Wysłana została wiadomość o treści: " + s);

            }
        }
        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }

        public void ComputingThread()
        {
            lock (agentCollection)
            {
                agentCollection.CollectionChanged += SwitchAction;
            }
        }

        public void SwitchAction(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {


            if (agentCollection.Last().Contains("cloud"))
            {
                File.WriteAllText("myLinks" + id + ".xml", agentCollection.Last());
                lock (Switch.linkDictionary)
                {
                    Console.WriteLine("Próba dodania do słownika konfiguracji agenta:" + id.ToString());

                    //  Switch.linkDictionary.Clear();
                    Switch.FillDictionary(id);
                }
            }
            else if (agentCollection.Last().Contains("clean_dictionary"))
            {
                Console.WriteLine("Czyszczę połączenia");
                Switch.linkDictionary.Clear();
            }



        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
