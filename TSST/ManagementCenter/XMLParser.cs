using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ManagementCenter
{
    /// <summary>
    /// do parsowania xml jak sama nazwa wskazuje
    /// i wyciagania z nich wiadomosci
    /// </summary>
    class XMLParser
    {
        string name;
        XmlDocument xmlDefault;
        /// <summary>
        /// Konstruktor
        /// </summary>
        public XMLParser(string name)
        {
            xmlDefault = new XmlDocument();
            this.name = name;
            //nie wiem czy to jest bezpieczne
            xmlDefault.Load(name);
        }

        public XMLParser(XmlDocument xmlDoc)
        {
            xmlDefault = xmlDoc;
        }

        /// <summary>
        /// sluzy do pobrania listy linkow jakie sa na tej sciezce, bedzie wykorzystywana ta lista w algo
        /// zestawiania sciezek
        /// </summary>
        /// <returns></returns>
        public List<Link> GetLinks()
        {
            int id;
            int nodeA;
            int nodeB;
            string status;
            int portIn;
            int portOut;
            int cost;
            int slotsAmount;
            int lenght;

            Program.nodes = new List<Node>();


            XmlNode node1;
            List<Link> link = new List<Link>();

            foreach (XmlNode nodePort in xmlDefault.SelectNodes("//port"))
            {
                id = Int32.Parse(nodePort.Attributes["id"].Value);

                node1 = nodePort.SelectSingleNode("node_a");
                nodeA = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("node_b");
                nodeB = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("status");
                status = node1.InnerText;

                node1 = nodePort.SelectSingleNode("port_in");
                portIn = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("port_out");
                portOut = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("cost");
                cost = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("slots_amount");
                slotsAmount = Int32.Parse(node1.InnerText);

                node1 = nodePort.SelectSingleNode("lenght");
                lenght = Int32.Parse(node1.InnerText);

                link.Add(new Link(id, nodeA, nodeB, slotsAmount, cost, status, lenght));

                //tu jest wielki cheat dodawania nodow, tak naprawde teraz nie bedziemy potrzebowali do nich configa
                lock (Program.nodes)
                {
                    if (!Program.nodes.Exists(x => x.number == nodeA))
                    {
                        Program.nodes.Add(new Node(nodeA));
                    }
                    if (!Program.nodes.Exists(x => x.number == nodeB))
                    {
                        Program.nodes.Add(new Node(nodeB));
                    }
                }


                Console.WriteLine("Wczytałem link o id: " + id);
            }


            return link;
        }

        ///nie w uzyciu, po cos robile to, ale juz nie pamietam po co
        public List<int> GetNodePorts(int nodeId)
        {
            List<int> ports = new List<int>();
            int i = 0;
            foreach (XmlNode nodePort in xmlDefault.SelectNodes("//node[@id=" + nodeId + "]/matrix_entry"))
            {
                ports.Add(Int32.Parse(nodePort.Attributes["num"].Value));
                Console.WriteLine(ports[i++]);
            }
            return ports;
        }





        /// <summary>
        /// tez nigdzie nie uzyty i juz nie wiem po co zrobiony
        /// Odczyt pliku XML i zapis portów do słownika
        /// <param name="filePath">ścieżka do pliku konfiguracyjnego</param>
        /// </summary>
        public Dictionary<int, string> ReadXml(string filePath)
        {
            Dictionary<int, string> configText = new Dictionary<int, string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode node in doc.SelectNodes("config/nodes/node"))
            {
                int id = int.Parse(node.Attributes["id"].Value);
                configText.Add(id, node.InnerXml);
            }

            foreach (XmlNode node in doc.SelectNodes("config/clients/client"))
            {
                int id = int.Parse(node.Attributes["id"].Value);
                configText.Add(id, node.InnerXml);
            }

            foreach (XmlNode node in doc.SelectNodes("config/cable_cloud"))
            {
                //chmura nie potrzebuje id, więc daję arbitralnie 0
                configText.Add(0, node.InnerXml);
            }
            return configText;
        }
    }
}
