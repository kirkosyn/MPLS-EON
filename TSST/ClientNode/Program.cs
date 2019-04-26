using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientNode
{
    /// <summary>
    /// Główna klasa programu
    /// </summary>
    class Program
    {
        public static int number;
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Nieużywane</param>
        /// 
        static void Main(string[] args)
        {
            number = Int32.Parse(args[0]);
            Console.WriteLine("client" + args[0]);

            //tworze port z ktorego laczy i slucha a potem wysyla
            Port p = new Port();
            p.CreateSocket("127.0.10." + args[0], 11001);
            p.Connect("127.0.11." + args[0], 11001);

            //watek wysylajacy
            Thread t1 = new Thread(new ThreadStart(p.SendThread));
            t1.Start();

            //watek sluchania agenta
            Agent agent = new Agent();
            string ip = "127.0.12." + args[0];
            agent.CreateSocket(ip, 11001);
            agent.Connect();

            //watek wysylania agenta
            Thread threadAgent = new Thread(new ThreadStart(agent.ComputingThread));
            threadAgent.Start();

            //wypisywanie polecen konsoli
            CLI.Promt();

        }
    }
}

