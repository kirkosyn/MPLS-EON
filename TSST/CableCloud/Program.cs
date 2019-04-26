using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CableCloud
{
    /// <summary>
    /// Główna klasa programu
    /// </summary>
    class Program
    {
        public static String nodeAmount = "0";
        static List<ClientCloud> client = new List<ClientCloud>();
        static List<Thread> clientThread = new List<Thread>();
        static List<Thread> connectClientThread = new List<Thread>();

        static List<NodeCloud> node = new List<NodeCloud>();
        static List<Thread> nodeThread = new List<Thread>();
        static List<Thread> connectNodeThread = new List<Thread>();

        static List<Agent> agent = new List<Agent>();
        static List<Thread> agentConnectThread = new List<Thread>();
        static List<Thread> agentComputingThread = new List<Thread>();

        //static ProcessThreadCollection clientThread;
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Nieużywane</param>
        static void Main(string[] args)
        {
            Console.WriteLine("CABLE CLOUD");

            int amountOfNodes = Int32.Parse(args[0]);
            int amountOfClients = Int32.Parse(args[1]);
            int amountOfManagers = Int32.Parse(args[2]);

            //tworzenie soketow dla managerow
            for (int i = 1; i <= amountOfManagers; i++)
            {
                agent.Add(new Agent(i));
                agent[i - 1].CreateSocket("127.0.30." + i.ToString(), 11001);

                agentConnectThread.Add(new Thread(new ThreadStart(agent[i - 1].Connect)));
                agentConnectThread[i - 1].Start();

                agentComputingThread.Add(new Thread(new ThreadStart(agent[i - 1].ComputingThread)));
                agentComputingThread[i - 1].Start();
            }


            //tworzenie soketow dla wezlow
            for (int i = 1; i <= amountOfNodes; i++)
            {
                try
                {
                    node.Add(new NodeCloud(i));
                    node[i - 1].CreateSocket("127.0.2." + i.ToString(), 11001);
                    node[i - 1].Connect("127.0.1." + i.ToString(), 11001);

                    //to dzialalo bez listy nie wiem czemu klienci nie dzialali jak nie bylo listy
                    Thread threadNode = new Thread(new ThreadStart(node[i - 1].SendThread));
                    threadNode.Start();


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Tworzę połączenia dla nodow, ex:" + ex.ToString());
                }

            }

            //tworzenie socketow dla klijentow
            for (int i = 1; i <= amountOfClients; i++)
            {
                try
                {
                    client.Add(new ClientCloud(i));
                    client[i - 1].CreateSocket("127.0.11." + i.ToString(), 11001);
                    connectClientThread.Add(new Thread(new ThreadStart(client[i - 1].Connect)));
                    connectClientThread[i - 1].Start();
                    clientThread.Add(new Thread(new ThreadStart(client[i - 1].SendThread)));
                    clientThread[i - 1].Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Tworzę połączenia dla klientow, ex:" + ex.ToString());
                }
            }



            //te read line musi byc inaczej wszystko sie konczy
            Console.ReadLine();



        }
    }
}
