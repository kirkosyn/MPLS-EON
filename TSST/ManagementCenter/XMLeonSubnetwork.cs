using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ManagementCenter
{
    class XMLeonSubnetwork
    {
        XmlDocument xmlDoc;
        ///nazwa dokumentu na ktorym pracujemy
        string name;

        public XMLeonSubnetwork()
        {
        }
        public XMLeonSubnetwork(XmlDocument xmlDoc)
        {
            this.xmlDoc = xmlDoc;
        }


        public XMLeonSubnetwork(string name)
        {
            this.name = name;
            xmlDoc = new XmlDocument();

        }

        public void CreateXml()
        {
            XmlNode config = xmlDoc.CreateElement("config");
            XmlNode myNetwork = xmlDoc.CreateElement("my_network");
            XmlNode clients = xmlDoc.CreateElement("clients");

            config.AppendChild(myNetwork);
            config.AppendChild(clients);

            xmlDoc.AppendChild(config);
            xmlDoc.Save(name);
        }


        public XMLeonSubnetwork(string name, string clientFile = null, string linksFile = null)
        {
            this.name = name;
            xmlDoc = new XmlDocument();
            XmlNode config = xmlDoc.CreateElement("config");
            XmlNode myNetwork = xmlDoc.CreateElement("my_network");
            XmlNode clients = xmlDoc.CreateElement("clients");
            clients.InnerText = clientFile;

            XmlNode links = xmlDoc.CreateElement("links");
            links.InnerText = linksFile;

            config.AppendChild(myNetwork);
            config.AppendChild(clients);
            config.AppendChild(links);

            xmlDoc.AppendChild(config);
            xmlDoc.Save(name);
        }


        public string GetClientFile()
        {
            string file;

            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("//clients");
            file = node.InnerText;
            return file;
        }

        public string GetLinkFile()
        {
            string file;

            //to powninno wskazywac na pierwsze wystapienie linka
            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("//links");
            file = node.InnerText;
            return file;
        }

        public void AddSubnetwork(int id, string address, string linksFile)
        {
            XmlNode sub = xmlDoc.CreateElement("subnetwork");
            XmlAttribute att = xmlDoc.CreateAttribute("id");
            att.Value = id.ToString();
            sub.Attributes.Append(att);

            XmlNode addressSub = xmlDoc.CreateElement("address");
            addressSub.InnerText = address;

            sub.AppendChild(addressSub);


            XmlNode node;
            node = xmlDoc.CreateElement("links");
            node.InnerText = linksFile;
            sub.AppendChild(node);
            XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//config");

            addTo.AppendChild(sub);
            //   xmlDoc.AppendChild(addTo);

            xmlDoc.Save(name);
        }

        public void AddSubnetwork(int id, string address, List<int> links, List<int> nodes)
        {
            xmlDoc.Load(name);

            XmlNode sub = xmlDoc.CreateElement("subnetwork");
            XmlAttribute att = xmlDoc.CreateAttribute("id");
            att.Value = id.ToString();
            sub.Attributes.Append(att);

            XmlNode addressSub = xmlDoc.CreateElement("address");
            addressSub.InnerText = address;

            sub.AppendChild(addressSub);


            XmlNode node;
            try
            {
                foreach (int i in links)
                {
                    //tu pojedynczo dodajemy wiec stad link a nie links
                    node = xmlDoc.CreateElement("link");
                    node.InnerText = i.ToString();
                    sub.AppendChild(node);
                }
            }
            catch { }
            try
            {
                foreach (int i in nodes)
                {
                    node = xmlDoc.CreateElement("node");
                    node.InnerText = i.ToString();
                    sub.AppendChild(node);
                }
            }
            catch
            {

            }

            XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//config");

            addTo.AppendChild(sub);
            //   xmlDoc.AppendChild(addTo);

            xmlDoc.Save(name);
        }

        public void AddSubSubNetwork(int idParent, int id, string address, List<int> links, List<int> nodes)
        {
            xmlDoc.Load(name);

            XmlNode sub = xmlDoc.CreateElement("subsubnetwork");
            XmlAttribute att = xmlDoc.CreateAttribute("id");
            att.Value = id.ToString();
            sub.Attributes.Append(att);

            XmlNode addressSub = xmlDoc.CreateElement("address");
            addressSub.InnerText = address;

            sub.AppendChild(addressSub);


            XmlNode node;
            foreach (int i in links)
            {
                //tu gdy dodajemy pojedynczo linki
                node = xmlDoc.CreateElement("link");
                node.InnerText = i.ToString();
                sub.AppendChild(node);
            }

            foreach (int i in nodes)
            {
                node = xmlDoc.CreateElement("node");
                node.InnerText = i.ToString();
                sub.AppendChild(node);
            }

            try
            {
                XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//subnetwork[@id=" + idParent + "]");
                addTo.AppendChild(sub);

                xmlDoc.Save(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public string StringSubnetwork(int id)
        {

            xmlDoc.Load(name);
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);

            string file;
            string readXML;

            XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//subnetwork[@id=" + id + "]");


            addTo.WriteTo(tx);
            file = sw.ToString();
            return file;
        }

        public string GetSubnetworkAddress(int id)
        {
            xmlDoc.Load(name);
            string file;

            XmlNode addTo = xmlDoc.DocumentElement.SelectSingleNode("//subnetwork[@id=" + id + "]/address");

            file = addTo.InnerText;
            return file;
        }

        public List<Subnetwork> GetSubnetworks()
        {
            List<Subnetwork> list = new List<Subnetwork>();

            xmlDoc.Load(name);
            XmlNodeList subnetworkList = xmlDoc.SelectNodes("//subnetwork");


            XmlNode node;
            XmlNodeList nodeList;
            int id;
            string address;
            List<int> links;
            List<int> nodes;
            string content;

            foreach (XmlNode sub in subnetworkList)
            {
                id = Int32.Parse(sub.Attributes["id"].Value);

                content = StringSubnetwork(id);

                node = sub.SelectSingleNode("address");
                address = node.InnerText;
                nodeList = sub.SelectNodes("/link");
                links = new List<int>();
                nodes = new List<int>();
                foreach (XmlNode link in nodeList)
                {
                    links.Add(Int32.Parse(link.InnerText));
                }
                nodeList = sub.SelectNodes("node");

                foreach (XmlNode node1 in nodeList)
                {
                    nodes.Add(Int32.Parse(node1.InnerText));
                }
                list.Add(new Subnetwork(id, address, links, nodes, content));
            }
            return list;
        }

    }
}
