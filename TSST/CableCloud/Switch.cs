using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CableCloud
{
    class Switch
    {
        // public static ObservableCollection<string> agentCollection = new ObservableCollection<string>();
        public static BlockingCollection<ObservableCollection<string>> nodeCollection = new BlockingCollection<ObservableCollection<string>>();
        public static BlockingCollection<ObservableCollection<string>> clientCollection = new BlockingCollection<ObservableCollection<string>>();

        // public static BlockingCollection<int> data = new BlockingCollection<int>();

        public static Dictionary<int, int> linkDictionary = new Dictionary<int, int>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">numer agenta ktory przyslal konfiga</param>
        public static void FillDictionary(int id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("myLinks" + id + ".xml");
            int inPort;
            int outPort;
            XmlNode node1;
            foreach (XmlNode node in doc.SelectNodes("cable_cloud/port"))
            {
                node1 = node.SelectSingleNode("port_in");
                inPort = Int32.Parse(node1.InnerText);
                node1 = node.SelectSingleNode("port_out");
                outPort = Int32.Parse(node1.InnerText);
                //   Console.WriteLine($"łączę {inPort} z {outPort}");
                try
                {
                    linkDictionary.Add(inPort, outPort);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Dla agenta: " + id + " nie udało się dodać portu: " + inPort + " ex:" + ex.ToString());
                }
            }

            Console.WriteLine("Uzupełniłem słownik portów dla agenta: " + id);

        }

        public static void SwitchBufer(string message)
        {
            int nodeOut;
            try
            {
                int start = message.IndexOf("<port>");
                int number = Int32.Parse(message.Substring(start + 6, 4));
                Console.WriteLine("Port: " + number);

                int linkOut = linkDictionary[number];
                //chodzi o to ze jak portOut ma format 4 cyfrowy to w port_In 2 pierwsze cyfry to skad idzie, a w port_out dokad
                //zatem w port out musimy wyluskac tysiace i sprowadzic je do jednosci
                nodeOut = (linkOut - (linkOut % 100)) / 100 - 10;
                if (nodeOut < 80)
                    Console.WriteLine("Przełączam węzły: " + nodeOut);
                else
                    Console.WriteLine("Przełączam na klienta: " + (nodeOut - 80));

                if (nodeOut < 80)
                {
                    lock (nodeCollection.ElementAt(nodeOut - 1))
                    {
                        nodeCollection.ElementAt(nodeOut - 1).Add(message);
                    }
                }
                else
                {
                    lock (clientCollection.ElementAt(nodeOut - 81))
                    {
                        clientCollection.ElementAt(nodeOut - 81).Add(message);
                        //     Console.WriteLine(clientCollection.ElementAt(nodeOut - 81).Last());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie znaleziono połączenia, ex:" + ex.ToString());
            }
        }

    }
}
