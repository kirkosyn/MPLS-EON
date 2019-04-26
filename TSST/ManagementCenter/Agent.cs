using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    class Agent
    {
        Socket mySocket;
        byte[] buffer;


        public Agent()
        {
        }
        string myIp;
        public void CreateSocket(string IP, int port)
        {

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
            buffer = new byte[30240];

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
                if (receivedMessage != "ping")
                {
                    Console.WriteLine();
                    Console.Write(this.GetTimestamp() + " : ");
                    if (receivedMessage.Contains("<subnetwork"))
                    {
                        int index = receivedMessage.IndexOf("id=");
                        index += 4;
                        string substr_id = receivedMessage.Substring(index, 1);
                        index = receivedMessage.IndexOf("links");
                        index += 6;
                        string substr_links = receivedMessage.Substring(index, 9);
                        Console.WriteLine("Agent otrzymał wiadomość o treści: id podsieci - " + substr_id + ", xml_links - " + substr_links);
                    }
                    else if (receivedMessage.Contains("<cable_cloud"))
                    {
                        Console.WriteLine("Agent otrzymał konfigurację łączy");
                    }
                    else if (receivedMessage.Contains("connection"))
                    {
                        int index = receivedMessage.IndexOf("port_in");
                        index += 8;
                        string substr_in = receivedMessage.Substring(index, 4);
                        index = receivedMessage.IndexOf("port_out");
                        index += 9;
                        string substr_out = receivedMessage.Substring(index, 4);
                        Console.WriteLine("Agent otrzymał wiadomość dotyczącą ścieżki: port_in - " + substr_in + ", port_out - " + substr_out);

                    }
                    else if (receivedMessage.Contains("check:"))
                    {
                        int index = receivedMessage.IndexOf("slot");
                        index += 5;
                        string substr_start = receivedMessage.Substring(index, 1);
                        index = receivedMessage.IndexOf("amount");
                        index += 7;
                        string substr_amount = receivedMessage.Substring(index, 1);

                        Console.WriteLine("RC otrzymało wiadomość od " + myIp + " dotyczącą możliwości rezerwacji szczeliny startowej " + substr_start + " oraz ich ilości " + substr_amount);
                    }
                    else if (receivedMessage.Contains("reserve:"))
                    {
                        Console.WriteLine("LRM otrzymało zadanie rezerwacji szczelin");
                    }
                    else if (receivedMessage.Contains("delete"))
                    {
                        int index = receivedMessage.IndexOf("port_in");
                        index += 8;
                        string substr_in = receivedMessage.Substring(index, 4);
                        index = receivedMessage.IndexOf("port_out");
                        index += 9;
                        string substr_out = receivedMessage.Substring(index, 4);
                        Console.WriteLine("Agent otrzymał żądanie usunięcia połączenia port_in " + substr_in + ", port_out - " + substr_out);
                    }
                    else
                        Console.WriteLine("Agent otrzymal wiadomosc: " + receivedMessage);

                    //   Console.WriteLine("Odebrano od agenta wiadomość o treści " + receivedMessage);


                    //Console.WriteLine("Od Agenta:\n " + receivedMessage);
                }

                lock (AgentSwitchingAction.agentCollection)
                {
                    AgentSwitchingAction.agentCollection.Add(receivedMessage);
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

        /// <summary>
        /// wysyla z powrotem do agenta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       // public void Send(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        public void Send(string message)
        {
            lock (mySocket)
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(message);

                mySocket.Send(sending);

                Console.Write(this.GetTimestamp() + " : ");
                if (message.Contains("<lenght"))
                {
                    int index = message.IndexOf("lenght");
                    index += 7;
                    string substr = message.Substring(index, 2);
                    Console.WriteLine("RC wysyła informację do " + myIp + " o długości ścieżki: " + substr);
                }
                else if (message.Contains("possible"))
                {
                    Console.WriteLine("Wysłano wiadomość do " + myIp + " o treści: LOL");
                }
                else if (message.Contains("error"))
                {
                    Console.WriteLine("Wysłano wiadomość o treści " + "dostępne okno");
                }
                else
                    Console.WriteLine("Agent wyslal wiadomość do " + myIp + " o treści: " + message);
            }
        }

        /// <summary>
        /// wielki cheat wysylam potem znacznik, ktory mowi co bedzie w  wiadomosci
        /// bo inaczej nie umiem
        /// </summary>
        /// <param name="stream"></param>
        public void Send(MemoryStream stream)
        {
            lock (mySocket)
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes("possible_window");
                // byte[] c = sending.Concat();


                mySocket.Send(stream.ToArray());
                mySocket.Send(sending);

                Console.Write(this.GetTimestamp() + " : ");
                Console.WriteLine("Wysłano wiadomość o treści: " + "dostępne okno");
            }
        }




        /// </summary>
        public void ComputingThread()
        {

            lock (AgentSwitchingAction.agentCollection)
            {
                AgentSwitchingAction.agentCollection.CollectionChanged += SwitchAction;
            }
        }

        /// <summary>
        /// sluzy do przelaczania akcji w zaleznosci od otrzymanej wiadomosci
        ///docelowo powinna zostac wywalona do innej klasy, by to ladnie zrobic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SwitchAction(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {

            if (AgentSwitchingAction.agentCollection.Last().Contains("error"))
            {
                try
                {
                    Send(AgentSwitchingAction.agentCollection.Last());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                AgentSwitchingAction.AgentAction(AgentSwitchingAction.agentCollection.Last(), this);
            }
        }

        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

    }

}
