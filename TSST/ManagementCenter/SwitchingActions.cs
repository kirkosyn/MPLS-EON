using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementCenter
{
    class SwitchingActions
    {


        /// <summary>
        /// z zwiazku z tym ze zakladam ze komp jest szybszy od nas
        /// zakaldam ze na raz bedzie zestawiana tylko jedna sciezka
        /// co bardzo ulatwia sprawe
        ///ale w zupelnosci nie emuluje prawdziwego zachowania sie sieci
        /// </summary>
        internal static Path pathToCount;

        /// <summary>
        /// jakie jest mozliwe okno dla jakiejs podsieci, ktora je przesyla
        /// i jest w rzucane w ta zmienna
        /// a nizej aktualizowana jest sciezka
        /// i zliaczane czy doszlo to info od wszystkich podsieci
        /// </summary>
        public static bool[] possibleWindow;

        /// <summary>
        /// stwierdza czy sciezka zostala zrekonfigurowana
        /// wartosc domyslna na true, bo na poczatku wszystko gra
        /// </summary>
        static bool reconfigured = true;

        /// <summary>
        /// ile sciezek zostalo nam jeszcze do zrekonfigurowania
        ///wykorzystywany przy naprawie na poziomie sieci
        /// </summary>
        static int toReconfigure = 0;

        /// <summary>
        /// zlicza ile wiadomosci possible window przyszlo
        /// by jak doszlo od wszystkich pod sieci stwierdzic ze 
        ///albo mozna rezultat wyslac wyzej
        ///albo juz mozna wybrac okno
        /// </summary>
        static int messageCounterPossibleWindow;

        static int messageCounterPossibleReservation;

        static int amountOfSlots;
        static int[] window;
        /// <summary>
        /// przechowuje informacje o startowym i koncowym kliencie
        /// data[0] = askingClient;
        /// data[1] = targetClient;
        /// </summary>
        static int[] data;

        /// <summary>
        /// przelaczanie tego co ma zrobic manager
        /// w zaleznosci co do niego doszlo
        /// </summary>
        /// <param name="message"></param>
        /// <param name="manager"></param>
        internal static void Action(string message, Manager manager)
        {

            //jezeli ma zostac polaczenie w podsieci czyl
            if (message.Contains("subconection"))
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Prośba o zestawienie połączenia w podsieci");
            }
            //ta wiadomosc moze przyjsc tylko do glownego managera
            //poniewaz do tych mniejszych zakladamy ze nie moga byc podlaczeni klijenci
            else if (message.Contains("connection:"))
            {
                //zerujemy licznik
                messageCounterPossibleWindow = 0;
                messageCounterPossibleReservation = 0;

                //data[0] = askingClient;
                //data[1] = targetClient;
                data = GetStartAndEndNode(message);

                lock (Program.nodes)
                {
                    lock (Program.links)
                    {
                        pathToCount = PathAlgorithm.dijkstra(Program.nodes, Program.links, data[0] + 80, data[1] + 80, false);
                    }
                }
                SendToSubnetworks(pathToCount);
            }
            //klijent prosi o usuniecie podsieci
            else if (message.Contains("delete"))
            {
                data = GetStartAndEndNode(message);
                string pathId = (data[0] + 80).ToString() + (data[1] + 80).ToString();
                pathToCount = Program.paths.Find(x => x.id == pathId);
                SendSubToDeleteConnection(pathToCount);
                pathToCount.ResetSlotReservation();
                lock (Program.paths)
                {
                    Program.paths.Remove(pathToCount);
                }

            }
            //gdy dostaje z podesieci wiadomosc jak dluga jest droga w podsieci
            //info jest potrzebne do wyliczenia ile slotow potrzebujemy
            else if (message.Contains("lenght"))
            {
                int lenght = GetLenght(message);
                pathToCount.lenght += lenght;
            }
            else if (message.Contains("possible_window"))
            {
                pathToCount.ChangeWindow(possibleWindow);
                messageCounterPossibleWindow++;

                //jezeli jest to najwyza sciezka i doszly juz wszystkie wiadomosci
                //minus 2 jest, bo na samej gorze sa jeszcze klijenci ich nie uwzgledniamy
                if (Program.isTheTopSub && messageCounterPossibleWindow == pathToCount.nodes.Count - 2)
                {
                    amountOfSlots = PathAlgorithm.AmountNeededSlots(pathToCount.lenght);
                    //returnWindow= new int[2] {startSlot,maxWindow };
                    window = pathToCount.FindMaxWindow();

                    bool isReservationPossible = pathToCount.IsReservingWindowPossible(amountOfSlots, window[0]);

                    if (isReservationPossible)
                    {
                        SendAskIfReservationIsPossible(window[0], amountOfSlots);
                    }
                    //to trzeba zrobic jakies inne polecania na zestawianie sciezko na okolo
                    else
                    {
                        //znaczy to ze w tym miejscu sie nie udalo zrobic rekonfiguracji
                        reconfigured = true;
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Nie można zestawić ścieżki");
                    }

                }
            }

            else if (message.Contains("possible_path"))
            {
                messageCounterPossibleReservation++;
                //jezeli jest na samym szczycie by wyslal nizej zadnia
                //minus dwa bo nie uwzgledniamu klientow
                //jezeli licznik wybil ze u wszystkich mozliwa jest rezerwacja okna to rozsylane jest prosba, by zarezerwowali to okno
                if (messageCounterPossibleReservation == pathToCount.nodes.Count - 2 && Program.isTheTopSub == true)
                {
                    pathToCount.ReserveWindow(amountOfSlots, window[0]);

                    SendSubToReserveWindow(window[0], amountOfSlots);
                    //data[1] target client
                    if (reconfigured == true)
                    {
                        SendClientsToReserveWindow(window[0], data[1]);
                    }

                    XMLeon xml = new XMLeon("path" + data[0] + data[1] + ".xml", XMLeon.Type.nodes);
                    pathToCount.xmlName = "path" + data[0] + data[1] + ".xml";
                    xml.CreatePathXML(pathToCount);

                    //dodawania sciezki do listy sciezek 
                    lock (Program.paths)
                    {
                        Program.paths.Add(pathToCount);
                    }

                    //jak jest przypadek z rekonfiguracja, gdy sie uda
                    if (reconfigured == false)
                    {
                        reconfigured = true;
                        //zmniejszanie liczby sciezek jakie pozostaly jeszcze do zrekonfigurowania
                        toReconfigure--;
                        //rozeslanie informacji do klijenta wysylajacego o zmianie sciezki
                        var targetClient = SwitchingActions.pathToCount.nodes.First().number - 80;
                        var message1 = "replace:<start_slot>" + SwitchingActions.pathToCount.startSlot + "</start_slot><target_client>" + targetClient + "</target_client>";

                        try
                        {

                            Program.managerClient[SwitchingActions.pathToCount.nodes.Last().number - 80 - 1].Send(message1);
                        }
                        catch (Exception ex)
                        {
                            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                            Console.WriteLine("Nie udało się wysłać ścieżki do klienta, ex: " + ex.ToString());
                        }
                    }

                    Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                    Console.WriteLine("Zestawianie ścieżki się powiodło");
                }
            }
            //jezeli zepsula sie podsiec, by naprawic to na wyzszym poziomie

            else if (message.Contains("error"))
            {
                Console.WriteLine("Do naprawy: " + toReconfigure);
                Thread.Sleep(3000 * toReconfigure);
                toReconfigure++;


                //tu jest maly cheat- taki ze numery podsieci sa takie same jak ich managerow
                //by nie parsowac ich wiadomosci ze wzgledu na numer
                //TODO zrobic ze podsiec wysyla tylko te scezki ktorych nie moze naprawic
                int deadSub = manager.number;
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Zepsuła się podsieć: " + manager.number);

                //ustawienie ze jak nie mozna w niej usunac sciezki to sie ustawia ze jest ona martwa, by algorytm dijxtry do niej
                //nie wchodzil
                Program.nodes.Find(x => x.number == deadSub).isAlive = false;

                string errorPathId = GetIdOfErrorPath(message);



                var path = Program.paths.Find(x => x.globalID == errorPathId);

                path.ResetSlotReservation();
                SendSubToDeleteConnection(path);




                //a tu zestawiamy od nowa
                //musza byc dwie petle, bo robimy sycnhronicznie zestawianie
                lock (Program.nodes)
                {
                    lock (Program.links)
                    {
                        SwitchingActions.pathToCount = PathAlgorithm.dijkstra(Program.nodes, Program.links, path.nodes.Last().number, path.nodes.First().number, false);
                    }

                    try
                    {
                        lock (Program.paths)
                        {
                            Program.paths.Remove(Program.paths.Find(x => x.globalID == errorPathId));
                        }

                    }
                    catch
                    {

                    }
                    if (SwitchingActions.pathToCount.endToEnd == false)
                    {
                        //jak nie udalo sie zrekonfigurwac zmniejszamy licznik
                        toReconfigure--;

                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Naprawa ścieżki jest niemożliwa");
                    }
                    else
                    {
                        //zerujemy licznik
                        messageCounterPossibleWindow = 0;
                        messageCounterPossibleReservation = 0;

                        reconfigured = false;
                        SendToSubnetworks(SwitchingActions.pathToCount);

                        //TODO potem do wodotryskow, ze jak sie jedna sciezka zepsuje to nie wylacza podsieci forever
                        /*
                        while(reconfigured==false)
                        {
                            Thread.Sleep(100);
                        }
                        Program.nodes.Find(x => x.number == deadSub).isAlive = true;*/
                    }

                }




            }
            //jezeli nie mozliwe jest zestawienie polaczenia w podsieci
            //TODO jak zestawienie polaczenia nie jest mozliwe przez jakas podscie to by poszlo inaczej
            else if (message.Contains("connection_impossible"))
            {
                //jak nie mozna w jakiejs podsieci zestawic to wtedy pomijamy to polaczenie
                //TODO zrobic ze jak nies niemozliwe to by szukac innego rozwiazania
                if (reconfigured == false)
                {
                    reconfigured = true;

                }
                Console.WriteLine();
            }
        }


        static string GetIdOfErrorPath(string message)
        {
            string id;
            int start, end;
            start = message.IndexOf("<error>") + 7;
            end = message.IndexOf("</error>");
            id = (message.Substring(start, end - start));

            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine("Zepsuta ścieżka o id: " + id);

            return id;
        }

        static void SendClientsToReserveWindow(int startSlot, int targetClient)
        {
            //sprawdzic czy indeksowanie OK jest
            for (int i = SwitchingActions.pathToCount.nodes.Count - 1; i >= 1; i--)
            {
                if (SwitchingActions.pathToCount.nodes[i].number > 80)
                {
                    string message1;
                    if (SwitchingActions.pathToCount.pathIsSet == true)
                    {
                        message1 = "<start_slot>" + SwitchingActions.pathToCount.startSlot + "</start_slot><target_client>" + targetClient + "</target_client>";
                    }
                    else
                    {
                        message1 = "zabraklo slotow";
                    }
                    try
                    {
                        Program.managerClient[SwitchingActions.pathToCount.nodes[i].number - 80 - 1].Send(message1);
                    }
                    catch
                    {
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Nie udało się wysłać ścieżki do klienta");
                    }
                }
            }
        }

        static void SendSubToReserveWindow(int startSlot, int amountOfSlots)
        {
            //sprawdzic czy indeksowanie OK jest
            for (int i = pathToCount.nodes.Count - 1; i >= 0; i--)
            {
                if (Program.isTheBottonSub == false && pathToCount.nodes[i].number < 80)
                {
                    string message1 = "reserve:<start_slot>" + startSlot + "</start_slot><amount>" + amountOfSlots + "</amount>";
                    lock (Program.subnetworkManager)
                    {
                        Program.subnetworkManager.Find(x => x.number == pathToCount.nodes[i].number).Send(message1);
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Wysyłam do podsieci prośbę o zarezerwowanie okna: " + pathToCount.nodes[i].number);
                    }
                }
            }
        }


        static void SendAskIfReservationIsPossible(int startSlot, int amountOfSlots)
        {
            //sprawdzic czy indeksowanie OK jest
            for (int i = pathToCount.nodes.Count - 1; i >= 0; i--)
            {
                if (Program.isTheBottonSub == false && pathToCount.nodes[i].number < 80)
                {
                    string message1 = "check:<start_slot>" + startSlot + "</start_slot><amount>" + amountOfSlots + "</amount>";
                    lock (Program.subnetworkManager)
                    {
                        Program.subnetworkManager.Find(x => x.number == pathToCount.nodes[i].number).Send(message1);
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Wysyłam pytanie do podsieci, czy są w stanie zarezerwować okna:" + pathToCount.nodes[i].number);
                    }
                }
            }
        }


        /// <summary>
        /// bierze dlugosc z wiadomosci ktora doszla od podsieci
        /// format wiadomosci
        /// <lenght>i tu jest dlugosc</lenght>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static int GetLenght(string message)
        {
            int lenght;
            int start, end;

            start = message.IndexOf("<lenght>") + 8;
            end = message.IndexOf("</lenght>");

            lenght = Int32.Parse(message.Substring(start, end - start));

            return lenght;
        }

        /// <summary>
        /// pobiera startowy i koncowy wezel
        /// czyli klienta proszacego i docelowego
        /// gdy to jest najwyzszyw ezel
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static int[] GetStartAndEndNode(string message)
        {
            int[] result = new int[2];
            int askingClient, targetClient;
            int start, end;

            start = message.IndexOf("<target_client>") + 15;
            end = message.IndexOf("</target_client>");
            // end = message.IndexOf("<port>");
            //13 stad ze //connection: konczy sie na 13 znaku
            //targetClient = Int32.Parse(message.Substring(13, end - 13));
            targetClient = Int32.Parse(message.Substring(start, end - start));

            Console.WriteLine("target client" + targetClient);
            start = message.IndexOf("<my_id>") + 7;
            end = message.IndexOf("</my_id>");
            askingClient = Int32.Parse(message.Substring(start, end - start));
            result[0] = askingClient;
            result[1] = targetClient;

            return result;
        }

        /// <summary>
        /// wysylanie podsieciom wiadomosi o to jaka maja sciezeke zestawic
        /// </summary>
        /// <param name="path"></param>
        public static void SendToSubnetworks(Path path)
        {
            if (path.endToEnd == true)
            {

                //sprawdzic czy indeksowanie OK jest
                for (int i = path.nodes.Count - 1; i >= 0; i--)
                {
                    if (Program.isTheBottonSub == false && path.nodes[i].number < 80)
                    {
                        string message1 = "connection<port_in>" + path.nodes[i].inputLink.id + "</port_in><port_out>" + path.nodes[i].outputLink.id + "</port_out><global_id>" + path.globalID + "</global_id>";
                        lock (Program.subnetworkManager)
                        {
                            Program.subnetworkManager.Find(x => x.number == path.nodes[i].number).Send(message1);
                            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                            Console.WriteLine("Wysyłam zadanie do podsieci: " + path.nodes[i].number);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// wysyla zadanie podsieciom jaka sciezke maja wywalic
        /// </summary>
        /// <param name="path"></param>
        public static void SendSubToDeleteConnection(Path path)
        {
            for (int i = path.nodes.Count - 1; i >= 0; i--)
            {
                if (Program.isTheBottonSub == false && path.nodes[i].number < 80)
                {
                    string message1 = "delete<port_in>" + path.nodes[i].inputLink.id + "</port_in><port_out>" + path.nodes[i].outputLink.id + "</port_out><global_id>" + path.globalID + "</global_id>";
                    lock (Program.subnetworkManager)
                    {
                        Program.subnetworkManager.Find(x => x.number == path.nodes[i].number).Send(message1);
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Wysyłam zadanie do podsieci: " + path.nodes[i].number);
                    }
                }
            }
        }



        /// <summary>
        /// gdy zdechnie wezel by od nowa zrekonfigurowac polaczenia i 
        /// rozsyla info wezlom co byly na poprzedniej sciezce by wywlaily co maja na nia
        /// potem wysyla klijentowi nowe info jak ma wysylac
        /// i wezlom co sa na tej nowej sciezce
        /// </summary>
        /// <param name="id"></param>
        internal static void NodeIsDead(int id)
        {
            string message1;
            var toReconfigure = Program.paths.FindAll(x => x.nodes.Contains(x.nodes.Find(y => y.number == id)));
            foreach (Path path in toReconfigure)
            {
                path.ResetSlotReservation();
            }
            foreach (Path path in toReconfigure)
            {
                System.Threading.Thread.Sleep(100);
                path.ResetSlotReservation();
                message1 = "remove:" + path.nodes.Last().number + path.nodes[0].number;
                foreach (Node node in path.nodes)
                {
                    if (node.number <= 80 && node.number != id)
                    {
                        lock (Program.managerNodes)
                        {
                            try
                            {
                                Program.managerNodes[node.number - 1].Send(message1);
                            }
                            catch
                            {
                                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                                Console.WriteLine("Nie udało się automatycznie usunąć wpisów");
                            }
                        }
                    }
                }

                Path pathForFunction;
                lock (Program.nodes)
                {
                    lock (Program.links)
                    {
                        pathForFunction = PathAlgorithm.dijkstra(Program.nodes, Program.links, path.nodes.Last().number, path.nodes.First().number, false);
                    }

                }

                if (pathForFunction.pathIsSet == true)
                {
                    lock (Program.paths)
                    {
                        try
                        {
                            //jezeli udalo sie zestawic nowe polaczenie to jest podmieniane
                            Program.paths[Program.paths.FindIndex(x => x == path)] = pathForFunction;
                            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                            Console.WriteLine("Zamieniłem ścieżkę");
                        }
                        catch
                        {
                            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                            Console.WriteLine("Nie udało się zamienić ścieżki");
                        }
                    }



                    var xml = new XMLeon(path.xmlName);

                    //rozeslanie informacji do klijenta wysylajacego o zmianie sciezki
                    var targetClient = pathForFunction.nodes.First().number - 80;
                    message1 = "replace:<start_slot>" + pathForFunction.startSlot + "</start_slot><target_client>" + targetClient + "</target_client>";

                    try
                    {

                        Program.managerClient[path.nodes.Last().number - 80 - 1].Send(message1);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                        Console.WriteLine("Nie udało się wysłać ścieżki do klienta, ex: " + ex.ToString());
                    }
                    //koniec rozsylo do klijenta

                    //taka indkesacja, bo bierzemy od konca i nie potrzebujemy do odbiorcy niczego wysylac
                    for (int i = pathForFunction.nodes.Count - 1; i >= 1; i--)
                    {
                        if (pathForFunction.nodes[i].number < 80)

                        {

                            message1 = xml.StringNode(pathForFunction.nodes[i].number);
                            Console.WriteLine(message1);
                            try
                            {
                                Program.managerNodes[pathForFunction.nodes[i].number - 1].Send(message1);
                            }
                            catch
                            {
                                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                                Console.WriteLine("Nie udało się wysłać ścieżki do węzła");
                            }
                        }
                    }
                }
                else
                {
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
                            Console.WriteLine("Nie udało się wywalić ścieżki");
                        }
                    }
                }
            }
        }




        /// <summary>
        /// gdy client prosi o usuniecie polaczenia
        /// sluzy do usuniecia sciezki z listy gdy jest o to prosba i rozeslania wiadomosci do wezlow by wywalily je ze swojej pamieci
        /// </summary>
        /// <param name="message"></param>
        static void DeleteConnection(string message)
        {
            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine("Prośba o usunięcie połączenia");
            int askingClient, targetClient;
            int start, end;


            end = message.IndexOf("<port>");
            //9 stad ze //delete: konczy sie na 9 znaku
            targetClient = Int32.Parse(message.Substring(9, end - 9));
            Console.WriteLine("target client" + targetClient);
            start = message.IndexOf("<my_id>") + 7;
            end = message.IndexOf("</my_id>");
            askingClient = Int32.Parse(message.Substring(start, end - start));

            Console.WriteLine("asking client" + askingClient);

            string id = (askingClient + 80).ToString() + (targetClient + 80).ToString();
            Path p;
            try

            {
                p = Program.paths.Find(x => x.id == id);
                Console.WriteLine(p.id);
                try
                {
                    //zwalnianie linkow
                    p.ResetSlotReservation();
                    foreach (Node node in p.nodes)
                    {
                        lock (Program.managerNodes)
                        {
                            if (node.number < 80)
                            {
                                string message1 = "remove:" + p.nodes.Last().number + p.nodes[0].number;
                                Program.managerNodes[node.number - 1].Send(message1);
                            }
                        }
                    }
                }
                catch
                {
                    Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                    Console.WriteLine("Nie udało się wysłać próśb o usunięcie wpisów");
                }

                try
                {

                    lock (Program.paths)
                    {

                        Program.paths.Remove(p);
                        // Console.WriteLine("cc");

                    }
                }
                catch
                {
                    Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                    Console.WriteLine("Nie udało się usunąć");
                }
            }
            catch
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Nie znaleziono takiej ścieżki do usunięcia");
            }

            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine("Polecenie usunięcia ścieżki obsłużone");
        }


    }
}
