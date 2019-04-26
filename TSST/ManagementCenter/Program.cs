using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementCenter
{
    /// <summary>
    /// Główna klasa programu
    /// </summary>
    class Program
    {
        /// <summary>
        /// okresla czy dany manager jest na samym dole sieci- czy juz nie ma pod nim podsieci
        /// jezeli ==true znaczy ze jest na samym dole
        /// </summary>
        public static bool isTheBottonSub = false;
        public static bool isTheTopSub = false;

        public static int amountOfnodes;
        public static int amountOfclients;
        public static int amountOfSubnetworks;

        public static List<Link> links;
        // tu w ich dodawaniu bedzie jeden wielki cheat bo bedzie szlo ono po wezlach jakie sa w linkach
        public static List<Node> nodes;
        public static List<Path> paths = new List<Path>();
        public static List<Subnetwork> subnetworksList = new List<Subnetwork>();


        public static List<Manager> managerNodes = new List<Manager>();
        public static List<Manager> managerClient = new List<Manager>();
        public static List<Manager> subnetworkManager;

        public static Manager managerCloud;

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Nieużywane</param>
        static void Main(string[] args)
        {
            //w tescie sie tworzy te sieci i inne xml, wiec raz odpalic
            //a potem zakomentowac
            //i znowu skompilowac

            //Tests.TestXML();

            /*    bool[] a = new[] { true, false };

                MemoryStream stream = new MemoryStream();
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);

                IFormatter formatter1= new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                var b = formatter1.Deserialize(stream);

                Console.WriteLine(b);*/


            Console.WriteLine("MANAGER:" + args[0]);
            subnetworkManager = new List<Manager>();

            //ustalenie wartosci ile mamy czego
            amountOfnodes = Int32.Parse(args[1]);
            amountOfclients = Int32.Parse(args[2]);
            amountOfSubnetworks = Int32.Parse(args[3]);



            //laczenie sie z chmura
            managerCloud = new Manager();
            managerCloud.CreateSocket("127.0.31." + args[0], 11001);


            //sie tu czasem wywalalo, to pomaga
            //laczenie sie z chmura
            while (true)
            {
                try
                {
                    managerCloud.Connect("127.0.30." + args[0], 11001);
                    break;
                }
                catch { }
            }


            Agent agent;

            if (args[0] == "1")
            {
                Console.WriteLine("Centralny Manager");
                isTheTopSub = true;

                for (int i = 2; i <= amountOfSubnetworks; i++)
                {
                    isTheBottonSub = true;
                    subnetworkManager.Add(new Manager(i));
                    subnetworkManager[i - 2].CreateSocket("127.0.21." + i, 11001);
                    subnetworkManager[i - 2].Connect("127.0.20." + i, 11001);

                }
                //laczenie sie z klientami
                for (int i = 1; i <= amountOfclients; i++)
                {
                    managerClient.Add(new Manager());
                    Console.WriteLine("Robię managera dla clienta " + "127.0.12." + i.ToString());

                    managerClient[i - 1].CreateSocket("127.0.13." + i.ToString(), 11001);
                    managerClient[i - 1].Connect("127.0.12." + i.ToString(), 11001);
                }
            }
            //jezeli nie jest to glowny manager to tworzymy agenta
            //polaczenia z nodami sa po tym jak agent dostanie info ktory plik koniguracyjny ma brac;
            else
            {
                agent = new Agent();

                agent.CreateSocket("127.0.20." + args[0].ToString(), 11001);
                Thread thread = new Thread(new ThreadStart(agent.Connect));
                thread.Start();
                Thread threadComputing = new Thread(new ThreadStart(agent.ComputingThread));
                threadComputing.Start();

            }


            //string do ktorego wczytujemy polecenie odbiorcy
            string choose;
            while (true)
            {
                //wypisanie polecen
                CLI.Prompt();

                choose = Console.ReadLine();
                if (choose == "1")
                {
                    if (Program.isTheTopSub)
                    {
                        CLI.ConfigureSubnetworks();
                    }

                }
                else if(choose == "2")
                {
                    if (Program.isTheBottonSub)
                    {
                        CLI.GetNodeFromNode(managerNodes);
                    }
                }
                else if (choose == "3")
                {
                   if(isTheBottonSub)
                    {
                        CLI.GetMatrixFromNode(managerNodes);
                    }
                }
                else if(choose=="4")
                {
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Nie ma takiej komendy");
                }
              
            }

        }
    }
}
