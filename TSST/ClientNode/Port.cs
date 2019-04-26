using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ClientNode
{
    class Port
    {
        Socket mySocket;

        EndPoint endRemote;
        byte[] buffer;
        public int client;

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

        public void Connect(string IP, int port)
        {
            string toIp = IP;
            int toPort;
            toPort = port;

            IPAddress ipAddress = IPAddress.Parse(toIp);

            endRemote = new IPEndPoint(ipAddress, toPort);
            mySocket.Connect(endRemote);

            Console.WriteLine("Połączono z chmurą");

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

                //wydrukowanie wiadomosci
                PrintMessage(receivedMessage);

                //usuniecie informacji charakterystycznej
                // receivedMessage=receivedMessage.Substring(receivedMessage.IndexOf("<address>"));

                //Console.WriteLine("Otrzymana wiadomosc:" + receivedMessage);
              
               // Console.WriteLine("Otrzymana zostala wiadomosc o tresci: " + receivedMessage);

                buffer = new byte[1024];
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
                    new AsyncCallback(MessageCallback), buffer);

            }
            catch (Exception ex)
            {

            }
        }
        public void Send(string message, int idOfNodeWeAreSendingTo)
        {
            try
            {
                int startSlot;
                lock (Agent.clientEonDictioinary)
                {
                    startSlot = Agent.clientEonDictioinary[idOfNodeWeAreSendingTo];
                }

                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];

                sending = enc.GetBytes(message + "<port>" + Agent.portOut + "</port>" + "<start_slot>" + startSlot + "</start_slot>");

                mySocket.Send(sending);
                Console.Write(this.GetTimestamp() + " : ");
                Console.WriteLine("Wysłana została wiadomość o treści: " + message+"\n");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie udało się wysłać: " + ex.ToString());
            }
        }

        /// <summary>
        /// sluzy do wysylania do managera przez noda
        /// </summary>
        /// <param name="message"></param>
        public void SendCommand(string message)
        {
            try
            {

                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(message + "<port>" + Agent.portOut + "</port>" + "<my_id>" + Program.number + "</my_id>");

                mySocket.Send(sending);
                Console.Write(this.GetTimestamp() + " : ");
                Console.WriteLine("Wysłana została wiadomość o treści: " + message+"\n");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie udało się wysłac: " + ex.ToString());
            }
        }



        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }


        public void SendThread()
        {
            CLI.SwitchCommands(this);
        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine();
            Console.Write(this.GetTimestamp() + " : ");
            Console.Write("Doszla wiadomosc:  ");
            if(message.Contains("port"))
            {
                int start=message.IndexOf("<port>");
                Console.WriteLine(message.Substring(0,start));
            }
        }
    }
}
