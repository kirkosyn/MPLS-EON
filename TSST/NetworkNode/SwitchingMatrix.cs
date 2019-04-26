using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkNode
{
    /// <summary>
    /// Pole komutacyjne
    /// </summary>
    class SwitchingMatrix
    {
        public static ObservableCollection<string> sendCollection = new ObservableCollection<string>();
        public static ObservableCollection<string> computingCollection = new ObservableCollection<string>();

        public static ObservableCollection<string> agentCollection = new ObservableCollection<string>();



        //pierwszy int to port wejsciowy, drugi to start slot, trzeci to port wyjsciowy
        public static Dictionary<int, Dictionary<int, int>> eonDictionary = new Dictionary<int, Dictionary<int, int>>();
        //pierwszy to start slot, drugi to port wyjsciowy
        static Dictionary<int, int> switchingDictionary = new Dictionary<int, int>();


        /// <summary>
        /// sluzy do wywalania wpisow ze slownika
        /// </summary>
        /// <param name="num"></param>
        public static void RemoveEonDictionary(int num)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("myNode" + Program.number + ".xml");
            int startSlot;
            XmlNode node1;

            try
            {
                //to w sumie mozna wywalic do parsera, bo tam jest tego miejsce zgodnie z konwencja
                XmlNode node = doc.SelectSingleNode("//node[@id=" + Program.number + "]/matrix_entry/connection[@num=" + num + "]");

                node1 = node.SelectSingleNode("start_slot");
                startSlot = Int32.Parse(node1.InnerText);

                switchingDictionary.Remove(startSlot);
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                Console.WriteLine("Usunąłem wpisy ze słownika ścieżki: " + num);

            }
            catch
            {
                Console.WriteLine("Nie było co usuwać dla: " + num);

            }
        }

        /// <summary>
        /// uzupelnia eono wy slownik
        /// </summary>
        public static void FillEonDictionary()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("myNodeconnection" + Program.number + ".xml");
            int inPort;
            int outPort;
            int startSlot;

            XmlNode node1;

            //to w sumie mozna wywalic do parsera, bo tam jest tego miejsce zgodnie z konwencja
            foreach (XmlNode nodePort in doc.SelectNodes("node/matrix_entry"))
            {

                inPort = Int32.Parse(nodePort.Attributes["num"].Value);
                foreach (XmlNode nodeConnection in nodePort.SelectNodes("connection"))
                {
                    node1 = nodeConnection.SelectSingleNode("start_slot");
                    startSlot = Int32.Parse(node1.InnerText);

                    node1 = nodeConnection.SelectSingleNode("port_out");
                    outPort = Int32.Parse(node1.InnerText);

                    switchingDictionary.Add(startSlot, outPort);
                   // Console.Write(DateTime.Now.ToString("HH:mm:ss") + " : ");
                    Console.WriteLine("  Port wejsciowy:  "+inPort + "  Start slot: " + startSlot + "  Port wyjsciowy: " + outPort);
                }
                if (!eonDictionary.ContainsKey(inPort))
                {
                    eonDictionary.Add(inPort, switchingDictionary);
                }
            }
            Console.Write(DateTime.Now.ToString("  HH:mm:ss") + " : ");
            Console.WriteLine("Dodałem wpisy ścieżki");
        }




        /// <summary>
        /// watek co tam trzyma to by to sie piknie switchowalo
        /// </summary>
        public static void ComputeThread()
        {
            lock (computingCollection)
            {
                computingCollection.CollectionChanged += ComputingEon;
            }
        }

        /// <summary>
        /// switchowanie eonowe
        /// tylko zmiana portow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ComputingEon(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string content;
            int inPort;
            int startSlot;
            int outPort;

            lock (computingCollection)
            {
                content = computingCollection.Last();

                inPort = Label.GetPort(content);
                startSlot = Label.GetStartSlot(content);
                outPort = eonDictionary[inPort][startSlot];

                content = Label.SwapPort(content, outPort);

                lock (sendCollection)
                {
                    sendCollection.Add(content);
                }
            }
        }
        
    }
}
