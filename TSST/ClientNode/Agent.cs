using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClientNode
{
    class Agent
    {
        Socket mySocket;
        byte[] buffer;

        //rzeczy do mpls
        public static ObservableCollection<string> agentCollection = new ObservableCollection<string>();
        //kluczem jest klijent z jakim sie laczymy, a wartoscia jego adres, port wyjsciowey
        public static Dictionary<int, Tuple<string, int>> clientDictioinary = new Dictionary<int, Tuple<string, int>>();

        //rzeczy do EON
        public static int portOut;
        //pierwsza wartosc to numer klijenta docelowego, druga to slot startowy
        public static Dictionary<int, int> clientEonDictioinary;// = new Dictionary<int, int>();

        public Agent()
        {
            // agentCollection = new ObservableCollection<string>();
            clientEonDictioinary = new Dictionary<int, int>();
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
            CLI.ConnectedAgent();
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
                if (receivedMessage.Contains("start_slot"))
                {
                    int index = receivedMessage.IndexOf("slot");
                    index += 5;
                    string substr_start = receivedMessage.Substring(index, 1);
                    index = receivedMessage.IndexOf("client");
                    index += 7;
                    string substr_client = receivedMessage.Substring(index, 1);

                    Console.WriteLine("Odebrana została od agenta wiadomość dotycząca szczeliny startowej " + substr_start + " oraz docelowego klienta " + substr_client);
                }
                else
                    Console.WriteLine("Odebrana została od agenta wiadomość o treści: " + receivedMessage);
                //Console.WriteLine("Od Agenta: \n" + receivedMessage);
                lock (agentCollection)
                {
                    agentCollection.Add(receivedMessage);
                   // Console.WriteLine(agentCollection.Last());
                   
                }

                buffer = new byte[1024];

                mySocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(MessageCallback), buffer);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Message callback execption, ex:" + ex + ToString());
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


        public void SendCommand(string message)
        {
            try
            {

                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] sending = new byte[1024];
                sending = enc.GetBytes(message + "<port>" + Agent.portOut + "</port>" + "<my_id>" + Program.number + "</my_id>");

                mySocket.Send(sending);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie udalo sie wyslac:" + ex.ToString());
            }
        }

        public void ComputingThread()
        {

            lock (agentCollection)
            {
              //  Console.WriteLine("agent");
                agentCollection.CollectionChanged += SwitchAction;
            }
        }

        public void SwitchAction(object sender, NotifyCollectionChangedEventArgs e)//(string message)
        {

            //zdaje sie ze z mpls i do wywalki
            /*if (agentCollection.Last().Contains("<client>"))
            {
                File.WriteAllText("myClient" + Program.number + ".xml", agentCollection.Last());
                clientDictioinary.Clear();
                FillDictionary();
            }*/
            if (agentCollection.Last().Contains("port_out:"))
            {
                GetPortOut(agentCollection.Last());
            }

            //gdy trzeba zmienic 
            //wazna kolejnosc else if, poniewaz to tez zawiera start slot, a nastepna nie zawiera juz replace
            else if (agentCollection.Last().Contains("replace:"))
            {
                ReplaceEonDictionary(agentCollection.Last());
            }

            else if (agentCollection.Last().Contains("<start_slot>"))
            {
                // Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaa");
                AddToEonDictionary(agentCollection.Last());
            }
            else if (agentCollection.Last().Contains("//connection:"))
            {

                SendCommand(agentCollection.Last());
            }
            else if (agentCollection.Last().Contains("//delete:"))
            {
                SendCommand(agentCollection.Last());
            }
            else
            {
                Console.WriteLine("Nie ma akcji dla tej wiadomosci");
            }

        }

        private void GetPortOut(string message)
        {
            portOut = Int32.Parse(message.Substring(9));
            Console.WriteLine("  Numer portu wychodzącego: " + portOut+"\n");
        }

        public void AddToEonDictionary(string message)
        {
            int startSlot;
            int targetClient;
            int start, end;

            start = message.IndexOf("<start_slot>") + 12;
            end = message.IndexOf("</start_slot>");

            startSlot = Int32.Parse(message.Substring(start, end - start));

            start = message.IndexOf("<target_client>") + 15;
            end = message.IndexOf("</target_client>");

            targetClient = Int32.Parse(message.Substring(start, end - start));
            clientEonDictioinary.Add(targetClient, startSlot);
            Console.WriteLine("  Dodaję do słownika wpis dla klienta o id: " + targetClient + " i o szczelinie start " + startSlot);
        }
        public void ReplaceEonDictionary(string message)
        {
            int startSlot;
            int targetClient;
            int start, end;

            start = message.IndexOf("<start_slot>") + 12;
            end = message.IndexOf("</start_slot>");

            startSlot = Int32.Parse(message.Substring(start, end - start));

            start = message.IndexOf("<target_client>") + 15;
            end = message.IndexOf("</target_client>");

            targetClient = Int32.Parse(message.Substring(start, end - start));
            clientEonDictioinary.Remove(targetClient);
            clientEonDictioinary.Add(targetClient, startSlot);
            Console.WriteLine("Wymieniłem wpis dla klienta o id: " + targetClient + " i o szczelinie start " + startSlot);
        }



        //zdaje sie ze z mpls i do wywalki
        public void FillDictionary()
        {

            XmlDocument doc = new XmlDocument();
            doc.Load("myClient" + Program.number + ".xml");
            int id;
            string address;
            int portOut;
            XmlNode node1;
            foreach (XmlNode client in doc.SelectNodes("clients/client"))
            {
                id = Int32.Parse(client.Attributes["id"].Value);


                node1 = client.SelectSingleNode("address");
                address = node1.InnerText;
                node1 = client.SelectSingleNode("port_out");
                portOut = Int32.Parse(node1.InnerText);
                //var t = new Tuple<address, portOut>;
                clientDictioinary.Add(id, new Tuple<string, int>(address, portOut));
                Console.WriteLine("Uzupełniam słownik klientow o klienta: " + id);
            }
        }
        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}


