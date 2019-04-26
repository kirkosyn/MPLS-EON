using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ManagementCenter
{
    /// <summary>
    /// xml do tworzenia eon owych rzeczy
    /// </summary>
    class XMLeon
    {
        //lista konstruktorow
        //

        //sprawdzic czy wszystkie sa tu wypisane
        //lista funkcji:
        //SavePathToFile
        //CreatePathXML
        //AddNode
        //StringNode 
        //StringCableLinks
        //GetClientPortOut
        //StringClients- testnac- te funkcje kiedy dzialaly
        //AddClient
        //AddMatrix
        //AddConnection
        //RemoveConnection
        //AddLink
        //ChangeLinkStatus
        //RemoveNode
        //RemoveClient-testnac
        //RemoveLink



        /// <summary>
        /// plik na ktorym zasowamy
        /// </summary>
        XmlDocument xmlDoc;
        ///nazwa dokumentu na ktorym pracujemy
        string name;

        /// <summary>
        /// typy plikow jakie morzemy utworzyc
        /// </summary>
        public enum Type { nodes, cable_cloud, clients };

        /// <summary>
        /// pomysl taki by bylodzielny xml na wezly, klijenty i lacza
        /// wazna by name podawac z koncowka xml
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type">czy lacze, wezly, </param>
        /// 
        public XMLeon(string name, Type type)
        {
            this.name = name;
            /*    try
                {
                    xmlDoc.Load(name);
                    xmlDoc.DocumentElement.ParentNode.RemoveAll();
                }
                catch
                {

                }*/

            xmlDoc = new XmlDocument();
            XmlNode config = xmlDoc.CreateElement("config");
            XmlNode nodes = xmlDoc.CreateElement(type.ToString());

            config.AppendChild(nodes);

            xmlDoc.AppendChild(config);
            xmlDoc.Save(name);
        }
        /// <summary>
        /// gdy chcemy wczytac juz istniejacy dokument
        /// </summary>
        /// <param name="name"></param>
        public XMLeon(string name)
        {
            xmlDoc = new XmlDocument();
            this.name = name;
            //nie wiem czy to jest bezpieczne
            xmlDoc.Load(name);
            //tu moze byc blad
            xmlDoc.Save(name);
        }

        /// <summary>
        /// zapis sciezki do pliku
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="path"></param>
        public static void SavePathToFile(int start, int end, Path path)
        {
            XMLeon xml = new XMLeon("path" + start.ToString() + end.ToString() + ".xml", XMLeon.Type.nodes);
            path.xmlName = ("path" + start + end + ".xml");
            xml.CreatePathXML(path);
        }

        /// <summary>
        /// sluzy do dodania w pliku jakiegos pustego xml typu nodes wpisow sciezki
        /// </summary>
        /// <param name="path"></param>
        public void CreatePathXML(Path path)
        {
            //nie wiem czemu to musi byc na 3 forach ale inaczej nie dziala xd
            /* for (int i = 1; i < path.nodes.Count - 1; i++)
             {
                 AddNode(path.nodes[i].number);
             }*/

            //testnac tego fora
            for (int i = 0; i < path.nodes.Count; i++)
            {
                if (path.nodes[i].number < 80)
                    AddNode(path.nodes[i].number);
            }
            for (int i = 0; i < path.nodes.Count; i++)
            {
                if (path.nodes[i].number < 80)
                    AddMatrix(path.nodes[i].inputLink.portIn, path.nodes[i].number);
            }
            for (int i = 0; i < path.nodes.Count; i++)
            {
                if (path.nodes[i].number < 80)
                    AddConnection(path.nodes.Last().number, path.nodes[0].number, path.nodes[i].number, path.nodes[i].inputLink.portIn, path.startSlot, path.endSlot, path.nodes[i].outputLink.portIn);
            }

        }


        public void AddNode(int id, string addressForCloud = null, string agent = null)
        {
            //zdaje sie ze te dwie linijki do wywalki
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);



            XmlNode node = xmlDoc.CreateElement("node");

            XmlNode cablePort = xmlDoc.CreateElement("cable_port");
            cablePort.InnerText = addressForCloud;

            XmlNode Agent = xmlDoc.CreateElement("agent");
            Agent.InnerText = agent;

            XmlAttribute attribute = xmlDoc.CreateAttribute("id");
            attribute.Value = id.ToString();
            node.Attributes.Append(attribute);
            if (addressForCloud != null && agent != null)
            {
                node.AppendChild(cablePort);
                node.AppendChild(Agent);
            }
            XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("nodes");
            addTo.AppendChild(node);
            xmlDoc.Save(name);

        }



        public string StringNode(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load(name);
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            try
            {
                start = readXML.IndexOf("<node id=\"" + id + "\">");

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

        public string StringCableLinks()
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load(name);
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            try
            {
                start = readXML.IndexOf("<cable_cloud");

                end = readXML.IndexOf("</cable_cloud>", start);
                file = readXML.Substring(start, end - start);
                file = file + "</cable_cloud>";
                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StringCableLinks: nie ma cable clouda, ex:" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// sluzy do pobrania portow wyjsciowych klijentow by pozniej kazdemu przeslac na jaki ma wysylac
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<int> GetClientPortOut(string name1)
        {
            List<int> portOut = new List<int>();

            XmlDocument doc = new XmlDocument();
            doc.Load(name1);

            //  Console.WriteLine(name1);
            XmlNode node1;
            // node1 = doc.SelectSingleNode("port_out");
            //Console.WriteLine(node1.InnerText);

            foreach (XmlNode client in doc.SelectNodes("//config/clients/client"))
            {


                node1 = client.SelectSingleNode("port_out");
                portOut.Add(Int32.Parse(node1.InnerText));
            }
            return portOut;
        }

        public string StringClients()
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load(name);
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            try
            {
                start = readXML.IndexOf("<clients");

                end = readXML.IndexOf("</clients>", start);
                file = readXML.Substring(start, end - start);
                file = file + "</clients>";
                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StringClients: nie ma klienta, ex:" + ex.ToString());
                return null;
            }
        }

        public void AddClient(int id, string clientAddress, int port_out)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode client = xmlDefault.CreateElement("client");
            XmlAttribute attribute = xmlDefault.CreateAttribute("id");
            attribute.Value = id.ToString();
            client.Attributes.Append(attribute);
            XmlNode address = xmlDefault.CreateElement("address");
            address.InnerText = clientAddress;
            client.AppendChild(address);
            XmlNode port = xmlDefault.CreateElement("port_out");
            port.InnerText = port_out.ToString();
            client.AppendChild(port);
            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("clients");
                addTo.AppendChild(client);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddClient: zły klient, ex:" + ex.ToString());
            }

        }


        public void AddMatrix(int num, int nodeNumber)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);


            XmlNode matrix = xmlDefault.CreateElement("matrix_entry");
            XmlAttribute attribute = xmlDefault.CreateAttribute("num");
            attribute.Value = num.ToString();
            matrix.Attributes.Append(attribute);

            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]");
                addTo.AppendChild(matrix);
                //  lock (xmlDefault)
                //{
                xmlDefault.Save(name);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddMatrix, ex:" + ex.ToString());
            }

        }
        public void AddConnection(int startNode, int endNode, int node, int matrix, int startSlot, int endSlot, int portOut)
        {

            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);

            XmlNode connection = xmlDefault.CreateElement("connection");
            XmlAttribute attribute = xmlDefault.CreateAttribute("num");
            // zmienic id polaczenia
            attribute.Value = startNode.ToString() + endNode.ToString() + startSlot.ToString();
            connection.Attributes.Append(attribute);

            XmlNode startSlotNode = xmlDefault.CreateElement("start_slot");
            startSlotNode.InnerText = startSlot.ToString();

            XmlNode endSlotNode = xmlDefault.CreateElement("end_slot");
            endSlotNode.InnerText = endSlot.ToString();

            XmlNode portNode = xmlDefault.CreateElement("port_out");
            portNode.InnerText = portOut.ToString();


            connection.AppendChild(startSlotNode);
            connection.AppendChild(endSlotNode);
            connection.AppendChild(portNode);
            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + node + "]/matrix_entry[@num=" + matrix + "]");
                addTo.AppendChild(connection);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddConnection, node:" + node + " matrix:" + matrix + " ex:" + ex.ToString());
            }
        }

        /// <summary>
        /// zdejmuje polaczenie w danym wezle, szuka po pierwszej szczelinie
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="matrix"></param>
        /// <param name="startSlot"></param>
        public void RemoveConnection(int nodeNumber, int matrixNumber, int startSlot)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            try
            {
                XmlNode rootConnection = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]");
                XmlNodeList nodeList = rootConnection.SelectNodes("connection");

                foreach (XmlNode child in nodeList)
                {

                    if (Int32.Parse(child.SelectSingleNode("start_slot").InnerText) == startSlot)
                    {
                        XmlNode parent = child.ParentNode;
                        //  XmlNode grandParent = parent.ParentNode;
                        parent.RemoveChild(child);
                        xmlDefault.Save(name);
                        break;
                    }

                }
            }
            catch (System.Xml.XmlException e)
            {
                Console.WriteLine("RemoveConnection ex:" + e.ToString());
            }




        }


        public void AddLink(int id, int nodeA, int nodeB, string status, int cost, int numberOfSlots, int lenght = 10)
        {

            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);

            XmlNode aNode = xmlDefault.CreateElement("node_a");
            aNode.InnerText = nodeA.ToString();
            XmlNode bNode = xmlDefault.CreateElement("node_b");
            bNode.InnerText = nodeB.ToString();

            //do standrdu wyznaczamy wartosc
            nodeA += 10;
            nodeB += 10;

            XmlNode port = xmlDefault.CreateElement("port");
            XmlAttribute attribute = xmlDefault.CreateAttribute("id");
            attribute.InnerText = id.ToString();
            port.Attributes.Append(attribute);
            XmlNode statusType = xmlDefault.CreateElement("status");
            statusType.InnerText = status;
            XmlNode linkIn = xmlDefault.CreateElement("port_in");
            linkIn.InnerText = nodeA.ToString() + nodeB.ToString();
            XmlNode linkOut = xmlDefault.CreateElement("port_out");
            linkOut.InnerText = nodeB.ToString() + nodeA.ToString();

            XmlNode costLink = xmlDefault.CreateElement("cost");
            costLink.InnerText = cost.ToString();
            XmlNode slots = xmlDefault.CreateElement("slots_amount");
            slots.InnerText = numberOfSlots.ToString();

            XmlNode len = xmlDefault.CreateElement("lenght");
            len.InnerText = lenght.ToString();

            port.AppendChild(aNode);
            port.AppendChild(bNode);

            port.AppendChild(statusType);
            port.AppendChild(linkIn);
            port.AppendChild(linkOut);
            port.AppendChild(costLink);
            port.AppendChild(slots);
            port.AppendChild(len);

            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("cable_cloud");
                addTo.AppendChild(port);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddLink, ex:" + ex.ToString());
            }
        }


        public void AddLink(int id, int nodeA, int nodeB, int portIn, int portOut, string status, int cost, int numberOfSlots, int lenght = 10)
        {

            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);

            XmlNode aNode = xmlDefault.CreateElement("node_a");
            aNode.InnerText = nodeA.ToString();
            XmlNode bNode = xmlDefault.CreateElement("node_b");
            bNode.InnerText = nodeB.ToString();


            XmlNode port = xmlDefault.CreateElement("port");
            XmlAttribute attribute = xmlDefault.CreateAttribute("id");
            attribute.InnerText = id.ToString();
            port.Attributes.Append(attribute);
            XmlNode statusType = xmlDefault.CreateElement("status");
            statusType.InnerText = status;
            XmlNode linkIn = xmlDefault.CreateElement("port_in");
            linkIn.InnerText = portIn.ToString();
            XmlNode linkOut = xmlDefault.CreateElement("port_out");
            linkOut.InnerText = portOut.ToString();

            XmlNode costLink = xmlDefault.CreateElement("cost");
            costLink.InnerText = cost.ToString();
            XmlNode slots = xmlDefault.CreateElement("slots_amount");
            slots.InnerText = numberOfSlots.ToString();

            XmlNode len = xmlDefault.CreateElement("lenght");
            len.InnerText = lenght.ToString();

            port.AppendChild(aNode);
            port.AppendChild(bNode);

            port.AppendChild(statusType);
            port.AppendChild(linkIn);
            port.AppendChild(linkOut);
            port.AppendChild(costLink);
            port.AppendChild(slots);
            port.AppendChild(len);

            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("cable_cloud");
                addTo.AppendChild(port);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddLink, ex:" + ex.ToString());
            }
        }




        public void ChangeLinkStatus(int portId, string status)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);

            try
            {
                XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("cable_cloud/port[@id=" + portId + "]/status");
                addTo.InnerText = status;
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ChangeLinkStatus ex:" + ex.ToString());
            }
        }


        public void RemoveNode(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            // XmlNode parent //= xmlDefault.DocumentElement.SelectSingleNode("nodes");
            try
            {
                XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + id + "]");
                XmlNode parent = child.ParentNode;
                parent.RemoveChild(child);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveNode, ex:" + ex.ToString());
            }

        }
        public void RemoveClient(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            try
            {
                XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//client[@id=" + id + "]");
                XmlNode parent = child.ParentNode;
                parent.RemoveChild(child);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveClient, ex:" + ex.ToString());
            }
        }

        public void RemoveLink(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            try
            {
                XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//cable_cloud/port[@id=" + id + "]");
                XmlNode parent = child.ParentNode;
                parent.RemoveChild(child);
                xmlDefault.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveLink, ex:" + ex.ToString());
            }
        }

    }
}
