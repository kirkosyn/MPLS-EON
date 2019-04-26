using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ManagementCenter
{
    /// <summary>
    /// obsluguje polecenia i wiadomosci jakie dostal agent
    /// </summary>
    class AgentSwitchingAction
    {
        internal static ObservableCollection<string> agentCollection = new ObservableCollection<string>();

        /// <summary>
        /// ma przechowywac polecenia jakie otrzymal od managera wyzej
        /// przychodzi polecenie i go tu zapisuje
        /// i potem jak idzie <order> tu polecenie </order> to wie ktorego polecenia sie to tyczy
        /// </summary>
        internal static BlockingCollection<string> requestCollection = new BlockingCollection<string>();

        static int[] messageData;

        internal static void AgentAction(string message, Agent agent)
        {
            //jezeli ma wyslac jeszcze dalej
            //wazna jest kolejnosc sprawdzania ifow, co zawiera wiadomosc, bo te wyzej wiadomosci moga sawierac te nizej, ale sa wazniejsze
            if (message.Contains("subsubnetwork"))
            {

            }

            //jezeli jest na samym dole hierarchi to nie ma juz wewnatrz podsicei
            else if (message.Contains("subnetwork"))
            {
                ConnectSubnetwork(message);
                Program.isTheBottonSub = true;
            }
            else if (message.Contains("connection"))
            {

                ConnectionRequest(message, agent);
            }
            else if (message.Contains("check"))
            {
                CheckRequest(message, agent);
            }
            else if (message.Contains("reserve"))
            {
                ReserveRequest(message);
                Program.paths.Add(SwitchingActions.pathToCount);
            }
            else if (message.Contains("delete"))
            {
                messageData = GetStartAndEndNode(message);

                string pathId = (messageData[0]).ToString() + (messageData[1]).ToString();
                SwitchingActions.pathToCount = Program.paths.Find(x => x.id == pathId);
                SendNodesToDeleteConnection(SwitchingActions.pathToCount);
                SwitchingActions.pathToCount.ResetSlotReservation();
                lock (Program.paths)
                {
                    Program.paths.Remove(SwitchingActions.pathToCount);
                }

                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Wysłałem do węzłów prośbę o usunięcie połączenia");
            }

        }

        /// <summary>
        /// funkcja uruchamiana wtedy gdy padnie jakis wezel
        /// </summary>
        /// <param name="id">numer noda ktory padl</param>
        internal static void NodeIsDead(int id)
        {
            var toReconfigure = Program.paths.FindAll(x => x.nodes.Contains(x.nodes.Find(y => y.number == id)));
            //czyszczenie zajetych zasobow
            foreach (Path path in toReconfigure)
            {
                path.ResetSlotReservation();
                SendNodesToDeleteConnection(path);
             //   Console.WriteLine("FIRST TRYInput port " + path.nodes.Last().inputLink.portIn + " Output port " + path.nodes.First().outputLink.portOut);
               // Console.WriteLine(path.xmlName);
            }

            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine("Posprzątane po awarii");

            foreach (Path path in toReconfigure)
            {
                lock (Program.nodes)
                {
                    lock (Program.links)
                    {
                        SwitchingActions.pathToCount = PathAlgorithm.dijkstra(Program.nodes, Program.links, path.nodes.Last().number, path.nodes.First().number, false);
                    }

                }
                if (!SwitchingActions.pathToCount.endToEnd)
                {

                    lock (agentCollection)
                    {
                        agentCollection.Add("<error>" + path.globalID + "</error>");
                    }
                    Console.WriteLine("Nie mozna ustawić innej ścieżki zapasowej w tej podsieci");
                }

                else
                {
                    //tu dodajemy do sciezki port na ktorej mamy z niej wyjechac i na ktory mamy wjechac
                    Link inPort = path.nodes.Last().inputLink;
                    Link outPort = path.nodes.First().outputLink;
                    SwitchingActions.pathToCount.nodes.First().outputLink = outPort;
                    SwitchingActions.pathToCount.nodes.Last().inputLink = inPort;

                    SwitchingActions.pathToCount.globalID = path.globalID;

                    Console.WriteLine("Input port " + inPort.portIn + " Output port  " + outPort.portOut);

                    Console.WriteLine("Start SLot: " + path.startSlot + " endSlot:" + path.endSlot);

                    //sprawdzamy czy mamy takie okno na tej sciezce jakie potrzebowalismy na starej
                    //tu moze cos byc namieszane ze slotami
                    //plus jeden proba bo np od 1 do 2 znajduja sie 2 liczby, a nie 2-1=1
                    if (SwitchingActions.pathToCount.IsReservingWindowPossible(path.endSlot - path.startSlot + 1, path.startSlot))
                    {

                        //tu plus jeden bo ta walona indeksacja
                        ReserveRequest(path.startSlot, path.endSlot - path.startSlot + 1);
                        Program.paths.Remove(Program.paths.Find(x => x == path));
                        Program.paths.Add(SwitchingActions.pathToCount);
                    }
                    else
                    {
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Brakuje szczelin, by zestawić połączenie");

                        //wysylanie do agenta wiadomosci ze sie nie udalo 
                        lock (agentCollection)
                        {
                            agentCollection.Add("<error>" + path.globalID + "</error>");
                        }

                        lock (Program.paths)
                        {
                            try
                            {
                                //jezeli nie udalo sie zestawic polaczenia to jest ono wywalane z listy polaczen
                                Program.paths.Remove(Program.paths.Find(x => x == path));
                            }
                            catch
                            {
                                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                                Console.WriteLine("Nie udało się usunąć ścieżki");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// wysylanie wiadomosci wezlom aby usunely polaczenie
        /// </summary>
        /// <param name="pathToCount"></param>
        private static void SendNodesToDeleteConnection(Path pathToCount)
        {
            string message1 = "remove:" + pathToCount.nodes.Last().number + pathToCount.nodes[0].number + pathToCount.startSlot;
            foreach (Node node in pathToCount.nodes)
            {
                if (node.number <= 80)
                {
                    lock (Program.managerNodes)
                    {
                        try
                        {
                            Program.managerNodes.Find(x => x.number == node.number).Send(message1);
                        }
                        catch
                        {
                            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                            Console.WriteLine("Nie udało się automatycznie usunąć wpisów");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// do obslugi zlecenia polaczenia
        /// ma na celu stwierdzenie czy jest mozliwe zestawienie sciezki EndToEnd
        /// </summary>
        /// <param name="message"></param>
        /// <param name="agent"></param>
        private static void ConnectionRequest(string message, Agent agent)
        {


            //jezeli jest to juz najnizsza podsiec to na jej poziomie juz konfigurujemy
            if (Program.isTheBottonSub == true)
            {
                //format wiadomosci
                //connection:<port_in>port</port_in><port_out>port<port_out>

                messageData = GetStartAndEndNode(message);


                var path = PathAlgorithm.dijkstra(Program.nodes, Program.links, messageData[0], messageData[1], false);
                //4 indeks to numer globalID
                path.globalID = messageData[4].ToString();
                if (path.endToEnd)
                {
                    //by byla tylko jedna sciezka ta globalna na ktorej pracujemy


                  //  Console.WriteLine("Istnieje połączenie EndToEnd");

                    //tu dodajemy do sciezki port na ktorej mamy z niej wyjechac i na ktory mamy wjechac
                    Link inPort = new Link(messageData[2]);
                    Link outPort = new Link(messageData[3]);

                    path.nodes.First().outputLink = outPort;
                    path.nodes.Last().inputLink = inPort;

                 /*   foreach (Path p in Program.paths)
                    {
                        Console.WriteLine("four TRYInput port " + p.nodes.Last().inputLink.portIn + "Output port  " + p.nodes.First().outputLink.portOut);
                        Console.WriteLine(p.xmlName);
                        Console.WriteLine("Start Slot:" + p.startSlot);

                    }
                    */
                    SwitchingActions.pathToCount = path;



                    string message1 = "<lenght>" + path.lenght + "</lenght>";

                    // string message1="<order>"+message+"</order><lenght>"+path.lenght+"</lenght>";
                    agent.Send(message1);

                    MemoryStream stream = new MemoryStream();
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, path.possibleWindow);



                    agent.Send(stream);

                }
                //
                else
                {

                }
            }
            //TODO
            // w przeciwnym razie slemy nizej, czyli jak sa podspodem jeszcze inne polaczenia bedziemy musieli slac nizej
            else
            {

            }
        }

        private static void CheckRequest(string message, Agent agent)
        {
            int[] data = GetStartAndAmountOfSlots(message);
            bool res;
            res = SwitchingActions.pathToCount.IsReservingWindowPossible(data[1], data[0]);
            if (res == true)
            {
                //wysylamy info 
                agent.Send("possible_path");
            }
            //jezeli nie bedzie mozliwa reserwacja trzeba bedzie to wyslac wyzej
            else
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Nie uda sie zarezerwować szczelin");
            }
        }



        /// <summary>
        /// sluzy do obslugi wiadomosci "reserve" mowiacej o to ktore okno mamy zarezerwowac
        ///ta wiadomosc moze przyjsc tylko gdy juz zostalo sprawdzone ze wszedzie te okno jest dostepne
        /// </summary>
        /// <param name="message"></param>
        public static void ReserveRequest(string message)
        {
            int[] data;
            data = GetStartAndAmountOfSlots(message);
            SwitchingActions.pathToCount.ReserveWindow(data[1], data[0]);
            XMLeon xml;
            //xml = new XMLeon("path" + messageData[0] + messageData[1] + ".xml", XMLeon.Type.nodes);

            xml = new XMLeon("path" + messageData[0] + messageData[1] + SwitchingActions.pathToCount.globalID + ".xml", XMLeon.Type.nodes);

            SwitchingActions.pathToCount.xmlName = ("path" + messageData[0] + messageData[1] + SwitchingActions.pathToCount.globalID + ".xml");
            xml.CreatePathXML(SwitchingActions.pathToCount);

            if (Program.isTheBottonSub == true)
            {
               /* foreach (Manager nod in Program.managerNodes)
                {
                    Console.WriteLine(nod.number);
                }*/
                foreach (Node node in SwitchingActions.pathToCount.nodes)
                {
                    string message1 = xml.StringNode(node.number);
                    Console.WriteLine(message1);
                    try
                    {
                        Console.WriteLine("Wezel: "+node.number);
                        Program.managerNodes.Find(x => x.number == node.number).Send(message1);
                    }
                    catch
                    {
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Nie udało się wysłać ścieżki do węzła");
                    }
                }
            }
        }

        public static void ReserveRequest(int startSlot, int neededSlots)
        {
            int[] data = { startSlot, neededSlots };
            //  data = GetStartAndAmountOfSlots(message);
            SwitchingActions.pathToCount.ReserveWindow(data[1], data[0]);
            XMLeon xml = new XMLeon("path" + messageData[0] + messageData[1] + SwitchingActions.pathToCount.globalID + ".xml", XMLeon.Type.nodes);
            //  XMLeon xml = new XMLeon("path" + messageData[0] + messageData[1] + ".xml");
            SwitchingActions.pathToCount.xmlName = ("path" + messageData[0] + messageData[1] + SwitchingActions.pathToCount.globalID + ".xml");
            xml.CreatePathXML(SwitchingActions.pathToCount);

            if (Program.isTheBottonSub == true)
            {
               /* foreach (Manager nod in Program.managerNodes)
                {
                    Console.WriteLine(nod.number);
                }*/
                foreach (Node node in SwitchingActions.pathToCount.nodes)
                {
                    string message1 = xml.StringNode(node.number);
                    Console.WriteLine(message1);
                    try
                    {
                        Console.WriteLine(" Wezel: "+node.number);
                        Program.managerNodes.Find(x => x.number == node.number).Send(message1);
                    }
                    catch
                    {
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Nie udało się wysłać ścieżki do węzła");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>start slot, amountOfSlots</returns>
        static int[] GetStartAndAmountOfSlots(string message)
        {
            int[] result = new int[2];


            int start, end;

            int startSlot, amountOfSlots;
            start = message.IndexOf("<start_slot>") + 12;
            end = message.IndexOf("</start_slot>");
            startSlot = Int32.Parse(message.Substring(start, end - start));

            start = message.IndexOf("<amount>") + 8;
            end = message.IndexOf("</amount>");
            amountOfSlots = Int32.Parse(message.Substring(start, end - start));

            result[0] = startSlot;
            result[1] = amountOfSlots;
            return result;
        }

        /// <summary>
        /// pobiera z wiadomosci info o rzadanym polaczeniu
        /// </summary>
        /// <param name="message"></param>
        /// <returns>
        /// 0 startNode
        /// 1 endNode
        /// 2 portIn
        /// 3 portOut
        /// 4 globalID
        /// </returns>
        static int[] GetStartAndEndNode(string message)
        {
            int[] result = new int[5];
            int start, end;
            int portIn, portOut;
            int startNode, endNode;

            int id;

            start = message.IndexOf("<port_in>") + 9;
            end = message.IndexOf("</port_in>");
            portIn = Int32.Parse(message.Substring(start, end - start));

            start = message.IndexOf("<port_out>") + 10;
            end = message.IndexOf("</port_out>");
            portOut = Int32.Parse(message.Substring(start, end - start));

            start = message.IndexOf("<global_id>") + 11;
            end = message.IndexOf("</global_id>");
            id = Int32.Parse(message.Substring(start, end - start));

            startNode = portIn % 100 - 10;
            endNode = portOut / 100 - 10;

            result[0] = startNode;
            result[1] = endNode;
            result[2] = portIn;
            result[3] = portOut;
            result[4] = id;

            Console.WriteLine();
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Parametry wiadomosci ");
            Console.WriteLine("  port_in: " + portIn + " port_out: " + portOut);
            Console.WriteLine("  start node: " + startNode + " end node: " + endNode);
            Console.WriteLine("  global ID: " + id);

            return result;
        }

        /// <summary>
        /// odpowiada za polaczenie sie z tymi wszystkimi wezlami na samy dole
        /// gdy juz wiemy ze nasz manger jest tym najnizszym managerem
        /// 
        /// </summary>
        /// <param name="message"></param>
        static void ConnectSubnetwork(string message)
        {
           // Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine(" Zadanie konfiguracji podsieci");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Niepoprawny format wiadomości, ex:" + ex.ToString());
            }
            XMLeonSubnetwork eonXml = new XMLeonSubnetwork(xmlDoc);
            string linkFile = eonXml.GetLinkFile();
            XMLParser xml = new XMLParser(linkFile);
            Program.links = xml.GetLinks();
            lock (Program.managerCloud)
            {
                Program.managerCloud.Send(XML.StringCableLinks(linkFile));

            }
            CLI.PrintConfigFilesSent();

            //laczenie sie z wezlami w podsieci
            lock (Program.managerNodes)
            {
                //i jest do indeksacji Program.managerNodes
                int i = 0;
                foreach (Node node in Program.nodes)
                {
                    Program.managerNodes.Add(new Manager(node.number));
                    Program.managerNodes[i].CreateSocket("127.0.4." + node.number, 11001);
                    while (true)
                    {
                        try
                        {
                            Program.managerNodes[i].Connect("127.0.3." + node.number, 11001);
                            break;
                        }
                        catch
                        {

                        }

                    }

                    //uruchuchomienie thread pingow do wezlow
                    try
                    {
                        Thread threadPing = new Thread(new ThreadStart(Program.managerNodes[i].PingThread));
                        threadPing.Start();
                    }
                    catch
                    {
                        Console.WriteLine("Nie udało się włączyć pinga");
                    }

                    i++;
                }


            }

        }

        public string GetTimestamp()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
