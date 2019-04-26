using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// zbior funkcji oblslugujacych nasz obecny format xml,plus pare rozszerzen 
/// staralem sie ze jak cos bedzie trzeba zmieniac, to zeby wystarczylo tylko pozmieniac pare linijek w kazdej funckji
/// dokumentacji z kazdej z osobna nie robie, bo moglbymm napisac tylko tyle ile mowi jej nazwa
/// @author Rawel
/// 
/// </summary>
/// 
namespace ManagementCenter
{



    /// <summary>
    /// funkcje:
    /// CreateXML- tworzy xml
    /// SetName-ustawia nazwe xml na ktorym pracujemy
    /// AddNode
    /// AddMatrix
    /// AddClient
    /// AddLink
    /// AddLabel
    /// StringNode
    /// StringCableLinks
    /// StringClients -przetestowac nowa
    /// ChangeLabelPort
    /// ChangeLabelAcction
    /// ChangeLabelIn
    /// ChangeLabelPush
    /// ChangeLabelSwap
    ///ChangeLinkStatus- zmienia stan czy dany link jest wlaczony czy nie, probonuje- "on" i "off
    ///RemoveNode
    ///RemoveClient
    ///RemoveLink
    ///RemoveLabel
    ///Test
    /// </summary>
    class XML
    {

        private static XmlDocument xmlDefault;
        private static string name;

        public static void CreateXML(string name)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode config = xmlDoc.CreateElement("config");
            XmlNode nodes = xmlDoc.CreateElement("nodes");
            nodes.InnerText = "";
            XmlNode clients = xmlDoc.CreateElement("clients");
            clients.InnerText = "";
            XmlNode cableCloud = xmlDoc.CreateElement("cable_cloud");
            cableCloud.InnerText = "";

            config.AppendChild(nodes);
            config.AppendChild(clients);
            config.AppendChild(cableCloud);
            xmlDoc.AppendChild(config);
            xmlDoc.Save(name);


        }
        public static void SetName(string filename)
        {
            name = filename;
        }
        public static bool Test()
        {
            try
            {
                XmlDocument xmlDefault = new XmlDocument();

                xmlDefault.Load(name);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Niepoprawna nazwa pliku");
                return false;
            }

        }
        public static void AddNode(int id, string port, string agent)
        {
            XmlDocument xmlDefault = new XmlDocument();

            xmlDefault.Load(name);
            XmlNode node = xmlDefault.CreateElement("node");
            XmlNode cablePort = xmlDefault.CreateElement("cable_port");
            cablePort.InnerText = port;

            XmlNode Agent = xmlDefault.CreateElement("agent");
            Agent.InnerText = agent;

            XmlAttribute attribute = xmlDefault.CreateAttribute("id");
            attribute.Value = id.ToString();
            node.Attributes.Append(attribute);
            node.AppendChild(cablePort);
            node.AppendChild(Agent);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("nodes");
            addTo.AppendChild(node);
            xmlDefault.Save(name);
        }

        public static string StringNode(int id)
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
            start = readXML.IndexOf("<node id=\"" + id + "\">");

            end = readXML.IndexOf("</node>", start);
            file = readXML.Substring(start, end - start);
            file = file + "</node>";
            return file;
        }
        public static string StringCableLinks(string name1)
        {
            XmlDocument xmlDefault = new XmlDocument();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;
            int start, end;
            xmlDefault.Load(name1);
            xmlDefault.WriteTo(tx);
            readXML = sw.ToString();
            start = readXML.IndexOf("<cable_cloud");

            end = readXML.IndexOf("</cable_cloud>", start);
            file = readXML.Substring(start, end - start);
            file = file + "</cable_cloud>";
            return file;
        }
        public static string StringClients()
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
            start = readXML.IndexOf("<clients");

            end = readXML.IndexOf("</clients>", start);
            file = readXML.Substring(start, end - start);
            file = file + "</clients>";
            return file;
        }




        public static void AddClient(int id, string clientAddress, int port_out)
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
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("clients");
            addTo.AppendChild(client);
            xmlDefault.Save(name);
        }


        public static void AddMatrix(int num, int nodeNumber)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode matrix = xmlDefault.CreateElement("matrix_entry");
            XmlAttribute attribute = xmlDefault.CreateAttribute("num");
            attribute.Value = num.ToString();
            matrix.Attributes.Append(attribute);

            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]");
            addTo.AppendChild(matrix);
            xmlDefault.Save(name);
        }


        public static void AddLabel(int nodeNumber, int matrixNumber, string acction, int label_in, int port, int swap = 0, int push = 0)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode labelIn = xmlDefault.CreateElement("label_in");
            XmlAttribute attribute = xmlDefault.CreateAttribute("label");
            attribute.Value = label_in.ToString();
            labelIn.Attributes.Append(attribute);
            XmlNode acctionType = xmlDefault.CreateElement("acction");
            acctionType.InnerText = acction;
            XmlNode swapType = xmlDefault.CreateElement("swap");
            swapType.InnerText = swap.ToString();
            XmlNode pushType = xmlDefault.CreateElement("push");
            pushType.InnerText = push.ToString();
            XmlNode portType = xmlDefault.CreateElement("port");
            portType.InnerText = port.ToString();

            labelIn.AppendChild(acctionType);
            labelIn.AppendChild(swapType);
            labelIn.AppendChild(pushType);
            labelIn.AppendChild(portType);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]");
            addTo.AppendChild(labelIn);
            xmlDefault.Save(name);
        }


        public static void AddLabel(int nodeNumber, int matrixNumber, string acction, int label_in, int port, string address, int swap = 0, int push = 0)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode labelIn = xmlDefault.CreateElement("label_in");
            XmlAttribute attribute = xmlDefault.CreateAttribute("label");
            attribute.Value = label_in.ToString();
            labelIn.Attributes.Append(attribute);
            XmlNode addressType = xmlDefault.CreateElement("address");
            addressType.InnerText = address.ToString();
            XmlNode acctionType = xmlDefault.CreateElement("acction");
            acctionType.InnerText = acction;
            XmlNode swapType = xmlDefault.CreateElement("swap");
            swapType.InnerText = swap.ToString();
            XmlNode pushType = xmlDefault.CreateElement("push");
            pushType.InnerText = push.ToString();
            XmlNode portType = xmlDefault.CreateElement("port");
            portType.InnerText = port.ToString();

            labelIn.AppendChild(addressType);
            labelIn.AppendChild(acctionType);
            labelIn.AppendChild(swapType);
            labelIn.AppendChild(pushType);
            labelIn.AppendChild(portType);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]");
            addTo.AppendChild(labelIn);
            xmlDefault.Save(name);
        }


        public static void AddLink(int id, int nodeA, int nodeB, string status)
        {
            nodeA += 10;
            nodeB += 10;
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);

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

            port.AppendChild(statusType);
            port.AppendChild(linkIn);
            port.AppendChild(linkOut);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("cable_cloud");
            addTo.AppendChild(port);
            xmlDefault.Save(name);
        }

        public static void ChangeLabelPort(int nodeNumber, int matrixNumber, int labelIn, int port)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]/label_in[@label=" + labelIn + "]/port");
            addTo.InnerText = port.ToString();
            xmlDefault.Save(name);
        }
        public static void ChangeLabelAcction(int nodeNumber, int matrixNumber, int labelIn, string acction)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]/label_in[@label=" + labelIn + "]/acction");
            addTo.InnerText = acction;
            xmlDefault.Save(name);
        }
        public static void ChangeLabelLabelIn(int nodeNumber, int matrixNumber, int labelIn, int newLabel)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]/label_in[@label=" + labelIn + "]");
            addTo.Attributes[0].Value = newLabel.ToString();
            xmlDefault.Save(name);
        }
        public static void ChangeLabelPush(int nodeNumber, int matrixNumber, int labelIn, int labelPush)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]/label_in[@label=" + labelIn + "]/push");
            addTo.InnerText = labelPush.ToString();
            xmlDefault.Save(name);
        }
        public static void ChangeLabelSwap(int nodeNumber, int matrixNumber, int labelIn, int labelSwap)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + nodeNumber + "]/matrix_entry[@num=" + matrixNumber + "]/label_in[@label=" + labelIn + "]/swap");
            addTo.InnerText = labelSwap.ToString();
            xmlDefault.Save(name);
        }


        public static void ChangeLinkStatus(int portId, string status)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);


            XmlNode addTo = xmlDefault.DocumentElement.SelectSingleNode("cable_cloud/port[@id=" + portId + "]/status");
            addTo.InnerText = status;
            xmlDefault.Save(name);
        }

        public static void RemoveNode(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            // XmlNode parent //= xmlDefault.DocumentElement.SelectSingleNode("nodes");
            XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//node[@id=" + id + "]");
            XmlNode parent = child.ParentNode;
            parent.RemoveChild(child);
            xmlDefault.Save(name);

        }
        public static void RemoveClient(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//client[@id=" + id + "]");
            XmlNode parent = child.ParentNode;
            parent.RemoveChild(child);
            xmlDefault.Save(name);
        }

        public static void RemoveLink(int id)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//cable_cloud/port[@id=" + id + "]");
            XmlNode parent = child.ParentNode;
            parent.RemoveChild(child);
            xmlDefault.Save(name);
        }
        public static void RemoveLabel(int nodeId, int matrixNum, int labelIn)
        {
            XmlDocument xmlDefault = new XmlDocument();
            xmlDefault.Load(name);
            XmlNode child = xmlDefault.DocumentElement.SelectSingleNode("//nodes/node[@id=" + nodeId + "]/matrix_entry[@num=" + matrixNum + "]/label_in[@label=" + labelIn + "]");
            XmlNode parent = child.ParentNode;
            parent.RemoveChild(child);
            xmlDefault.Save(name);
        }
    }
}
