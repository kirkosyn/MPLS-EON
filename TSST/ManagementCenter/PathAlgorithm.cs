using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{

    class PathAlgorithm
    {
        public static Path dijkstra(List<Node> nodes, List<Link> links, int start, int end, bool direction)
        {
            Console.WriteLine();
            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
            Console.WriteLine("Wykonuję algorytm dijkstry");
            //sciezka ktorej szukamy
            Path path = new Path();
            path.id = start.ToString() + end.ToString();
            //jezeli jest to najwyzszy level to Id globalne jest takie same jak na tym poziomie
            if (Program.isTheTopSub)
            {
                path.globalID = path.id;
            }

            //resetowanie statusu nodow z informacji ze sa na sciezce, z poprzedniego zestawiania
            Node.ResestConnectionStatus(nodes);

            //ustawiana na start na bardzo duzo liczbe
            double cheapestPath; ;

            //zmienna do obliczen, mowi ktory node jest w tej chwili rozwazany
            int nodeNumber = 0;

            //w ktorym aktualnie wezle jestesmy
            int actualNode = start;

            //ilosc slotow jaka bedzie potrzebna dla danej sciezki
            int amountOfSlots;

            //na jakim slocie zacznie sie okno i jaka jest jego maksymalna wielkosc
            int[] window;

            //zmienne sluzace do indeksowania po nodach
            int index;
            int index2;


            //ustawienie wartosci startowego noda
            //znalezienie jego indeksu
            try
            {
                index = nodes.IndexOf(nodes.Find(x => x.number == start));


                //chyba chodzi o to, ze sam jest poprzednikiem siebie
                nodes[index].previousNode = start;
                //dotarcie do pierwszego nic nie kosztuje
                nodes[index].costToGetHere = 0;
                //i jest juz na sciezce
                nodes[index].connected = true;
            }
            catch
            {
                //jak go nie znalzl to pewnie znaczy ze gonie ma
                Console.WriteLine();
            }


            for (int n = 0; n < nodes.Count; n++) //zmienione z petli nisekonczonej na przypadek gdy jakis wezel jest niepoloczony
            {
                for (int i = 0; i < links.Count; i++)
                {
                    if (links[i].nodeA == actualNode)
                    {
                        //try jest tutaj gdyz pozniej sa zabijane juz jakies wezly, a linki wziaz zyja
                        //wiec tak jest prosciej sprawdzic czy jeszcze jest taki wpis w liscie nodow
                        try
                        {
                            index = nodes.IndexOf(nodes.Find(x => x.number == links[i].nodeB));

                            index2 = nodes.IndexOf(nodes.Find(x => x.number == actualNode));
                            //jezeli oba zyja to znaczy nie sa wylaczone moze na nich wykonywac algorytm
                            if (nodes[index].isAlive && nodes[index2].isAlive)
                            {
                                int slotsCost = links[i].usedSlots.Where(c => c).Count();
                                if (nodes[index].costToGetHere > links[i].cost + slotsCost + nodes[index2].costToGetHere)
                                {
                                    //dodajemy dodatkowy koszt, żeby omijać linki gdzie są użyte szczeliny
                                    
                                    nodes[index].costToGetHere = links[i].cost + slotsCost + nodes[index2].costToGetHere;
                                    nodes[index].previousNode = links[i].nodeA;
                                    nodes[index].inputLink = links[i];
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                }
                //ustawiana na start na maksymalna wartosc int
                cheapestPath = 2147483647;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].connected == false && nodes[i].costToGetHere < cheapestPath)
                    {
                        cheapestPath = nodes[i].costToGetHere;

                        //indeksuje od zera, by sie nie powalilo
                        nodeNumber = nodes[i].number;
                    }
                }
                //zaznaczamy, ze najtanszy 
                try
                {
                    index = nodes.IndexOf(nodes.Find(x => x.number == nodeNumber));
                    nodes[index].connected = true;
                    actualNode = nodeNumber;
                }
                catch
                {

                }

                if (actualNode == end)
                {
                   // Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                    Console.WriteLine("Znalazłem polaczenie EndToEnd");
                    //ustawiamy tutaj ze sciezka zostala znaleziona
                    path.endToEnd = true;
                    break;
                }
            }
            if (path.endToEnd == true)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    index = nodes.IndexOf(nodes.Find(x => x.number == actualNode));
                    path.nodes.Add((Node)nodes[index].Clone());
                    path.connection.Add(nodes[index].inputLink);

                    path.ChangeWindow(nodes[index].inputLink);
                    path.hops++;
                    path.lenght += nodes[index].inputLink.lenght;

                    //cofamy sie po sciezce
                    actualNode = nodes[index].previousNode;

                    //tu przypisujemy wyjscia nodow, wyjscie aktualnego jest wejsciem poprzedniego
                    index2 = nodes.IndexOf(nodes.Find(x => x.number == actualNode));
                    nodes[index2].outputLink = nodes[index].inputLink;

                    if (actualNode == start)
                    {
                        //jest tu dodawanie, bo cofanie sie po sciezce jest przed ifem
                        index = nodes.IndexOf(nodes.Find(x => x.number == actualNode));
                        path.nodes.Add((Node)nodes[index].Clone());


                        //jak ostatni to nie bedzie mial poprzedniego wiec raczej z tad tego tu nie bedzie
                        path.hops++;


                      //  Console.WriteLine();
                        Console.WriteLine("Parametry sciezki:");
                        int[] pathWindow = path.FindMaxWindow();
                      //  Console.WriteLine("Window start: " + pathWindow[0] + " Window Size:" + pathWindow[1]);
                        Console.WriteLine(" Hops:" + path.hops);
                        break;

                    }
                }
               
                Console.WriteLine(" Przebieg sciezki:");
                for (int i = 0; i < path.nodes.Count; i++)
                {
                    Console.WriteLine("  Wezel: "+path.nodes[i].number);

                    try
                    {
                        Console.WriteLine("   InputLink: " + path.nodes[i].inputLink.id);
                    }
                    catch (Exception ex)
                    { }
                    try
                    {
                        Console.WriteLine("   OutputLink: " + path.nodes[i].outputLink.id);
                    }
                    catch (Exception ex)
                    { }


                }
                Console.WriteLine(" Dlugosc sciezki:" + path.lenght);

                // amountOfSlots=AmountNeededSlots(path.lenght);
                // window= path.FindMaxWindow();
                // path.ReserveWindow(amountOfSlots,window[0],window[1]);
                /*
                XMLeon xml = new XMLeon("path" + start + end + ".xml", XMLeon.Type.nodes);
                path.xmlName = ("path" + start + end + ".xml");
                xml.CreatePathXML(path);*/
            }
            else
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Nie udało się zestawić ścieżki\n");
            }
            return path;
        }

        /// <summary>
        /// w zaleznosci od dlugosci sciezki 
        /// </summary>
        /// <param name="lengthOfPath"></param>
        /// <returns></returns>
        internal static int AmountNeededSlots(int lengthOfPath)
        {
            int amountNeeded = 0;

            //granice sa przyjete przezemnie arbitralnie
            if (lengthOfPath < 10)
            {
                amountNeeded = 1;
                Console.WriteLine("Wykorzystana modulacja: 16QAM");
                Console.WriteLine("Ilość potrzebnych szczelin: " + 1);
            }
            else if (lengthOfPath < 20)
            {
                amountNeeded = 2;
                Console.WriteLine("Wykorzystana modulacja: 8QAM");
                Console.WriteLine("Ilość potrzebnych szczelin: " + 2);
            }
            else if (lengthOfPath < 30)
            {
                amountNeeded = 3;
                Console.WriteLine("Wykorzystana modulacja: QPSK");
                Console.WriteLine("Ilość potrzebnych szczelin: " + 3);
            }
            else
            {
                amountNeeded = 4;
                Console.WriteLine("Wykorzystana modulacja: BSK");
                Console.WriteLine("Ilość potrzebnych szczelin: " + 4);
            }

            return amountNeeded;
        }

    }
}