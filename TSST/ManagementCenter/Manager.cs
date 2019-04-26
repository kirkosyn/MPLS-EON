using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    /// <summary>
    /// Zarządza węzłami
    /// </summary>
    class Manager
    {
        Socket mySocket;

        EndPoint endRemote;
        byte[] buffer;

        string myIp;
        int myport;

        public int number;

        public Manager()
        {
        }

        public Manager(int number)
        {
            this.number = number;
        }


        public void CreateSocket(string IP, int port)
        {

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
            buffer = new byte[1024];
            Console.WriteLine("Połączono z adresem: " + IP);

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


                try
                {
                    SwitchingActions.possibleWindow = (bool[])DeserializeFromStream(auxtrim);

                    Console.WriteLine("Deserializacja");
                }
                catch
                {
                    // Console.WriteLine("Nie udalo sie deserializacja");
                }
                try
                {
                    string receivedMessage = encoding.GetString(auxtrim);

                    Console.WriteLine();
                    Console.Write(this.GetTimestamp() + " : ");

                    if (receivedMessage.Contains("<target_client"))
                    {
                        int index = receivedMessage.IndexOf("my_id");
                        index += 6;
                        string substr_id = receivedMessage.Substring(index, 1);
                        index = receivedMessage.IndexOf("_client");
                        index += 8;
                        string substr_client = receivedMessage.Substring(index, 1);
                        Console.WriteLine("CC otrzymal connection request o zestawieniu połączenia od klienta " + substr_id + " do klienta " + substr_client);
                    }
                    else if (receivedMessage.Contains("<lenght"))
                    {
                        int index = receivedMessage.IndexOf("lenght");
                        index += 7;
                        string substr = receivedMessage.Substring(index, 2);
                        Console.WriteLine("RC otrzymał informację o długości ścieżki: " + substr);
                    }
                    else if (receivedMessage.Contains("possible"))
                    {
                        
                        Console.WriteLine("RC otrzymał informację, że jest możliwa rezerwacja");
                    }
                    else if (receivedMessage.Contains("error"))
                    {

                        Console.WriteLine("CC otrzymał informację o LOLu");
                    }
                    else
                        Console.WriteLine("Manager otrzymal wiadomosc: " + receivedMessage);

                    try
                    {
                        SwitchingActions.Action(receivedMessage, this);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Zła akcja: " + receivedMessage);
                        Console.WriteLine(ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd odkodowywania");


                }



                buffer = new byte[1024];
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
                    new AsyncCallback(MessageCallback), buffer);

            }
            catch (Exception ex)
            {

            }
        }



        public static object DeserializeFromStream(byte[] receivedData)
        {
            MemoryStream stream = new MemoryStream(receivedData);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return objectType;
        }

        public void Send(string message)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] sending = new byte[1024];
            sending = enc.GetBytes(message);

            mySocket.Send(sending);

            if (!message.Contains("ping"))
            {
                Console.Write(this.GetTimestamp() + " : ");
                if (message.Contains("<subnetwork"))
                {
                    int index = message.IndexOf("id=");
                    index += 4;
                    string substr_id = message.Substring(index, 1);
                    index = message.IndexOf("links");
                    index += 6;
                    string substr_links = message.Substring(index, 9);
                    Console.WriteLine("Manager wysłał wiadomość na adres " + myIp + " o treści: id podsieci - " + substr_id + ", xml_links - " + substr_links);
                }
                else if (message.Contains("<cable_cloud"))
                {
                    Console.WriteLine("Manager wysłał konfigurację łączy na adres " + myIp);
                }
                else if (message.Contains("port_out:"))
                {
                    Console.WriteLine("Manager wysłał wiadomość na adres " + myIp + " dotyczącą portu wyjściowego " + message);
                }
                else if (message.Contains("node id"))
                {
                    int index = message.IndexOf("node id");
                    index += 9;
                    string substr_in = message.Substring(index, 2);
                    if (substr_in.ElementAt(1) == '\"')
                    {
                        substr_in = substr_in.ElementAt(0).ToString();
                    }
                    Console.WriteLine("LRM uzupełnia informację o parametrach połączenia dla węzła " + substr_in);
                }
                else if (message.Contains("connection"))
                {
                    int index = message.IndexOf("port_in");
                    index += 8;
                    string substr_in = message.Substring(index, 4);
                    index = message.IndexOf("port_out");
                    index += 9;
                    string substr_out = message.Substring(index, 4);
                    Console.WriteLine("Manager wysłał wiadomość na adres " + myIp + " dotyczącą ścieżki: port_in - " + substr_in + ", port_out - " + substr_out);

                }
                else if (message.Contains("check:"))
                {
                    int index = message.IndexOf("slot");
                    index += 5;
                    string substr_start = message.Substring(index, 1);
                    index = message.IndexOf("amount");
                    index += 7;
                    string substr_amount = message.Substring(index, 1);

                    Console.WriteLine("RC wysłał zapytanie na adres " + myIp + " dotyczącą możliwości rezerwacji szczeliny startowej " + substr_start + " oraz ich ilości " + substr_amount);
                }
                else if (message.Contains("reserve:"))
                {
                    Console.WriteLine("LRM rezerwuje szczeliny...");
                }
                else if (message.Contains("start_slot:"))
                {
                    int index = message.IndexOf("slot");
                    index += 5;
                    string substr_start = message.Substring(index, 1);
                    index = message.IndexOf("client");
                    index += 7;
                    string substr_client = message.Substring(index, 1);

                    Console.WriteLine("CC poinformowało " + myIp + " o szczelinie startowej " + substr_start + " oraz o docelowym kliencie " + substr_client);
                }
                else if (message.Contains("<start_slot"))
                {
                    int index = message.IndexOf("slot");
                    index += 5;
                    string substr_start = message.Substring(index, 1);
                    index = message.IndexOf("client");
                    index += 7;
                    string substr_client = message.Substring(index, 1);

                    Console.WriteLine("CC poinformowało " + myIp + " o szczelinie startowej " + substr_start + " oraz o docelowym kliencie " + substr_client);
                }
                else if (message.Contains("delete"))
                {
                    int index = message.IndexOf("port_in");
                    index += 8;
                    string substr_in = message.Substring(index, 4);
                    index = message.IndexOf("port_out");
                    index += 9;
                    string substr_out = message.Substring(index, 4);
                    Console.WriteLine("Manager wysłał na adres " + myIp + " żądanie usunięcia połączenia port_in " + substr_in + ", port_out - " + substr_out);
                }
                else if (message.Contains("remove"))
                {
                    Console.WriteLine("LRM czyści parametry połączenia dla " + myIp);
                }
                else if (message.Contains("replace"))
                {
                    int index = message.IndexOf("slot");
                    index += 5;
                    string substr_start = message.Substring(index, 1);
                    index = message.IndexOf("client");
                    index += 7;
                    string substr_client = message.Substring(index, 1);

                    Console.WriteLine("CC poinformowało " + myIp + " o zmienionej szczelinie startowej " + substr_start + " oraz o docelowym kliencie " + substr_client);
                }
                else
                    Console.WriteLine("Manager wysłał wiadomość na adres " + myIp + " o treści: " + message);
            }
        }

        public void disconnect_Click()
        {
            mySocket.Disconnect(true);
            mySocket.Close();
        }


        /// <summary>
        /// jest wywolywany jako watek i sprawdza czy wezly jeszcze zyja
        /// jak jakis zdechnie catch powinnien to wylapac
        /// i wtedy powinny zostac wycofane stare wpisy
        /// ustawione nowe
        /// i rozeslane
        /// </summary>
        public void PingThread()
        {
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(5000);
                    Send("ping");
                }
                catch
                {
                    Console.Write(this.GetTimestamp() + " : ");
                    Console.WriteLine("\nWęzeł: " + number + "  jest nieaktywny");

                    ////zamiast wywalac ustawiamy ze jest wylaczony
                    //Program.nodes.Find(x => x.number == number).isAlive=false;

                    //wersja z wywalniem noda
                    var item = Program.nodes.SingleOrDefault(x => x.number == number);
                    Program.nodes.Remove(item);

                    AgentSwitchingAction.NodeIsDead(number);
                    break;
                }
            }

        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

    }
}
