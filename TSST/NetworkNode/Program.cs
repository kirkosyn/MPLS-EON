using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Główna klasa programu
    /// </summary>
    class Program
    {
        public static int number;
        private static string[] ArgToIP(string arg)
        {
            int id = int.Parse(arg);
            number = id;
            // id = 2 * id + 10;

            return new string[] { "127.0.1." + id.ToString(), "127.0.3." + id.ToString() };
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">numer wezla</param>


        static void Main(string[] args)
        {

            Console.WriteLine("node" + args[0]);
            //Console.WriteLine("node number "+args[0]);

            string[] ips = ArgToIP(args[0]);
            Console.WriteLine("adres dla chmury: " + ips[0]);
            Console.WriteLine("adres dla agenta: " + ips[1]);

            Port port = new Port();
            Agent agent = new Agent();
            port.CreateSocket(ips[0], 11001);
            //port.CreateSocket(ips[0], 11002);
            agent.CreateSocket(ips[1], 11001);

            port.Connect();
            agent.Connect();

            Thread threadPort = new Thread(new ThreadStart(port.SendThread));
            Thread threadAgent = new Thread(new ThreadStart(agent.ComputingThread));
            Thread computeThread = new Thread(new ThreadStart(SwitchingMatrix.ComputeThread));

            threadAgent.Start();
            threadPort.Start();
            computeThread.Start();

            XMLParser.AddNode("myNode" + Program.number + ".xml", ips[0], ips[1]);

            //!!!!!!!!!!!!
            //bardzo wazne bez tego wyciaganie infromacji z labeli nie bedzie dzialac !!!!
            Label.setMask();

            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();

        }

    }
}
