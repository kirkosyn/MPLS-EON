using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetworkNode
{
    /// <summary>
    /// nie jest to tylko parser, robi wszystko co ma byc robione na xml
    /// </summary>
    class XMLParser
    {

        public static void AddNode(string name, string addressForCloud = null, string agent = null)
        {

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(name);
                xmlDoc.DocumentElement.ParentNode.RemoveAll();
            }
            catch
            { }

            XmlNode node = xmlDoc.CreateElement("node");

            XmlNode cablePort = xmlDoc.CreateElement("cable_port");
            cablePort.InnerText = addressForCloud;

            XmlNode Agent = xmlDoc.CreateElement("agent");
            Agent.InnerText = agent;

            XmlAttribute attribute = xmlDoc.CreateAttribute("id");
            attribute.Value = Program.number.ToString();
            node.Attributes.Append(attribute);
            if (addressForCloud != null && agent != null)
            {
                node.AppendChild(cablePort);
                node.AppendChild(Agent);
            }
            xmlDoc.AppendChild(node);
            xmlDoc.Save(name);
        }

        public static void RemoveConnection(string name, int num)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(name);
            try
            {
                XmlNode child = xmlDoc.SelectSingleNode("//node[@id=" + Program.number + "]/matrix_entry/connection[@num=" + num + "]");
                var parent = child.ParentNode;
                parent.RemoveChild(child);
                xmlDoc.Save(name);
                Console.WriteLine("Wpis usunięty");
            }
            catch
            {
            }
        }

        public static void AddConnection(string name, string message)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(name);

            XmlDocument xmlMessage = new XmlDocument();
            // var xxx = XDocument.Parse(message);
            // xmlMessage.PreserveWhitespace = false;
            File.WriteAllText("myNodeconnection" + Program.number + ".xml", SwitchingMatrix.agentCollection.Last());

            xmlMessage.Load("myNodeconnection" + Program.number + ".xml");


            XmlNode node, node1;

            node = xmlMessage.SelectSingleNode("/node/matrix_entry");

            try
            {
                int matrix = Int32.Parse(node.Attributes["num"].Value);
                XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//node[@id=" + Program.number + "]/matrix_entry[@num=" + matrix + "]");
               // Console.WriteLine("addtoforst   " + addTo.InnerXml);
                node = xmlMessage.SelectSingleNode("//node[@id=" + Program.number + "]/matrix_entry[@num=" + matrix + "]/connection");
               // Console.WriteLine(" node1first        " + node.InnerXml);
                node1 = xmlDoc.ImportNode(node, true);
                addTo.AppendChild(node1);
                xmlDoc.Save(name);
            }
            catch (Exception ex)
            {
                try
                {
                    node = xmlMessage.SelectSingleNode("/node/matrix_entry");
                    //xmlDoc.ImportNode(node,true);
                    //Console.WriteLine(node.InnerText);
                    XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//node[@id=" + Program.number + "]");
                    node1 = xmlDoc.ImportNode(node, true);
                 //   Console.WriteLine("addTO   " + addTo.InnerXml);
                  //  Console.WriteLine("node1:   " + node1.InnerXml);
                    addTo.AppendChild(node1);
                    xmlDoc.Save(name);
                }
                catch (Exception ex1)
                {
                    Console.WriteLine("Second:" + ex1.ToString());
                }
                //Console.WriteLine("first:" + ex.ToString());
            }
        }


        /// <summary>
        /// robi stringa z pliku konfiguracyjnego
        /// 
        /// </summary>
        /// <returns></returns>
        public static string StringNode()
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load("myNode" + Program.number + ".xml");
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            try
            {
                start = readXML.IndexOf("<node id=\"" + Program.number + "\">");

                end = readXML.IndexOf("</node>", start);
                file = readXML.Substring(start, end - start);
                file = file + "</node>";
                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine("nie ma wezlow, ex:" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// zwraca dla danego wezla zawartosc dla danego portu
        /// </summary>
        /// <param name="number">numer portu ktorego szukamy</param>
        /// <returns></returns>
        public static string StringMatrix(int number)
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load("myNode" + Program.number + ".xml");
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            try
            {
                //znajduje gdzie jest poczatek w xml informacji o danym numerze
                start = readXML.IndexOf("<matrix_entry num=\"" + number + "\">");

                //szuka konca tej informacji, szuka poczynajac od miejsca gdzie sie zaczela (start)
                end = readXML.IndexOf("</matrix_entry>", start);

                file = readXML.Substring(start, end - start);
                //niestety wycinajac tracimy znacznik konca informacji, wiec go teraz dodajemy
                file = file + "</matrix_entry>";
                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine("nie ma matrixa, ex:" + ex.ToString());
                //jezeli cos sie pochrzanilo  to zwracamy info ze nie ma takiego portu
                return "nie ma takiego portu";
            }

        }

        /// <summary>
        /// Słownik portów wejścia/wyjścia
        /// label_in, label_out, port_out
        /// </summary>
        public Dictionary<int, Tuple<int, int>> portTable;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public XMLParser()
        {
            portTable = new Dictionary<int, Tuple<int, int>>();
        }

        /// <summary>
        /// Odczyt pliku XML i zapis portów do słownika
        /// <param name="filePath">ścieżka do pliku konfiguracyjnego</param>
        /// </summary>
        public void ReadXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(filePath);

            foreach (XmlNode node in doc.SelectNodes("matrix_entry"))
            {
                var label_in = Int32.Parse(node.SelectSingleNode("label_in").InnerText);
                var label_out = Int32.Parse(node.SelectSingleNode("label_out").InnerText);
                var port_out = Int32.Parse(node.SelectSingleNode("port_out").InnerText);

                portTable.Add(label_in, new Tuple<int, int>(label_out, port_out));
            }
        }
    }
}
