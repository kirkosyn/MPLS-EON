using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    /// <summary>
    /// stworzylem ta klase by gdzies wrzucic te wszystkie test xml, ktore nieustannie robie
    /// by je tutaj tworzyc a nie po roznych dziwnych galeziach
    /// </summary>
    class Tests
    {

        /// <summary>
        /// tworzenie xml i takie tam ich testowanie
        /// 
        /// </summary>
        public static void TestXML()
        {
            //subnetowork content

            //klijenci
            XMLeon client1 = new XMLeon("subclient.xml", XMLeon.Type.clients);
            client1.AddClient(1, "127.0.0.1", 9111);
            client1.AddClient(2, "127.0.0.2", 9211);
            client1.AddClient(3, "127.0.0.3", 9326);
            client1.AddClient(4, "127.0.0.4", 9426);

            //linki
            XMLeon linksgen = new XMLeon("sub1links.xml", XMLeon.Type.cable_cloud);
            linksgen.AddLink(9111, 81, 2, 9111, 1191, "on", 1, 14);
            linksgen.AddLink(9211, 82, 2, 9211, 1191, "on", 1, 14);
            linksgen.AddLink(2693, 5, 83, 2693, 9326, "on", 1, 14);
            linksgen.AddLink(2694, 5, 84, 2694, 9426, "on", 1, 14);

            linksgen.AddLink(1415, 2, 3, 1415, 1514, "on", 1, 14);
            linksgen.AddLink(1419, 2, 4, 1419, 1914, "on", 1, 14);
            linksgen.AddLink(1823, 3, 5, 1823, 2318, "on", 1, 14);
            linksgen.AddLink(2223, 4, 5, 2223, 2322, "on", 1, 14);

            linksgen = new XMLeon("sub2links.xml", XMLeon.Type.cable_cloud);
            linksgen.AddLink(1112, 1, 2, "on", 1, 14);
            linksgen.AddLink(1113, 1, 3, "on", 1, 14);
            linksgen.AddLink(1214, 2, 4, "on", 1, 14);
            linksgen.AddLink(1314, 3, 4, "on", 1, 14);

            linksgen = new XMLeon("sub3links.xml", XMLeon.Type.cable_cloud);
            linksgen.AddLink(1516, 5, 6, "on", 1, 14);
            linksgen.AddLink(1517, 5, 7, "on", 1, 14);
            linksgen.AddLink(1618, 6, 8, "on", 1, 14);
            linksgen.AddLink(1718, 7, 8, "on", 1, 14);

            linksgen = new XMLeon("sub4links.xml", XMLeon.Type.cable_cloud);
            linksgen.AddLink(1920, 9, 10, "on", 1, 14);
            linksgen.AddLink(1921, 9, 11, "on", 1, 14);
            linksgen.AddLink(2022, 10, 12, "on", 1, 14);
            linksgen.AddLink(2122, 11, 12, "on", 1, 14);

            linksgen = new XMLeon("sub5links.xml", XMLeon.Type.cable_cloud);
            linksgen.AddLink(2324, 13, 14, "on", 1, 14);
            linksgen.AddLink(2325, 13, 15, "on", 1, 14);
            linksgen.AddLink(2426, 14, 16, "on", 1, 14);
            linksgen.AddLink(2526, 15, 16, "on", 1, 14);

            XMLeonSubnetwork sub = new XMLeonSubnetwork("subnetwork1.xml", "subclient.xml", "sub1links.xml");
            sub.AddSubnetwork(2, "", "sub2links.xml");
            sub.AddSubnetwork(3, "", "sub3links.xml");
            sub.AddSubnetwork(4, "", "sub4links.xml");
            sub.AddSubnetwork(5, "", "sub5links.xml");
            //end of subnetwork content


            XMLeon client = new XMLeon("client.xml", XMLeon.Type.clients);
            client.AddClient(1, "127.0.0.1", 9111);
            client.AddClient(2, "127.0.0.2", 9211);
            client.AddClient(3, "127.0.0.3", 9321);
            client.AddClient(4, "127.0.0.4", 9421);
            client.AddClient(5, "127.0.0.1", 11);
            client.AddClient(6, "111", 33);
            // client.RemoveClient(3);




            XMLeon nodes = new XMLeon("nodes.xml", XMLeon.Type.nodes);
            nodes.AddNode(1, "111", "3333");
            nodes.AddNode(2, "111", "3333");
            nodes.AddNode(3, "111", "3333");

            nodes.AddNode(4, "111", "3333");
            nodes.AddNode(5, "111e", "3333");
            nodes.AddNode(6, "11q1", "3333");
            nodes.AddNode(7, "11p1", "3333");
            nodes.RemoveNode(3);

            nodes.AddMatrix(3, 2);
            nodes.AddMatrix(3, 4);
            nodes.AddMatrix(11, 1);
            nodes.AddMatrix(13, 2);
            nodes.AddMatrix(23, 2);
            nodes.AddMatrix(3, 5);
            nodes.AddMatrix(23, 6);
            nodes.AddMatrix(93, 2);
            nodes.AddMatrix(31, 1);
            nodes.AddMatrix(3, 1);


            nodes.RemoveConnection(2, 13, 2);
            nodes.RemoveConnection(1, 11, 2);

            XMLeon links = new XMLeon("links.xml", XMLeon.Type.cable_cloud);

            links.AddLink(9111, 81, 1, "on", 1, 14);
            links.AddLink(9211, 82, 1, "on", 1, 14);

            links.AddLink(1112, 1, 2, "on", 1, 14);
            links.AddLink(1114, 1, 4, "on", 22, 14);

            links.AddLink(1215, 2, 5, "on", 22, 14);
            links.AddLink(1213, 2, 3, "on", 1, 14);

            links.AddLink(1316, 3, 6, "on", 1, 14);
            links.AddLink(1314, 3, 4, "on", 19, 14);

            links.AddLink(1417, 4, 7, "on", 22, 14);
            links.AddLink(1411, 4, 1, "on", 1, 14);

            links.AddLink(1516, 5, 6, "on", 22, 14);
            links.AddLink(1518, 5, 8, "on", 1, 14);

            links.AddLink(1617, 6, 7, "on", 22, 14);
            links.AddLink(1615, 6, 5, "on", 1, 14);

            links.AddLink(1719, 7, 9, "on", 22, 14);

            links.AddLink(1819, 8, 9, "on", 22, 14);

            links.AddLink(1920, 9, 10, "on", 22, 14);

            links.AddLink(2093, 10, 83, "on", 22, 14);
            links.AddLink(2094, 10, 84, "on", 22, 14);


            // links.AddLink(9111, 81, 1, "on", 22, 14);
            //links.AddLink(1112, 1, 2, "on", 22, 14);
            // links.AddLink(1213, 2, 3, "on", 22, 14);
            // links.AddLink(1392, 2, 82, "on", 22, 14);

            /* links.AddLink(1191, 1, 81, "on", 22, 14);
             links.AddLink(1211, 2, 1, "on", 22, 14);
             links.AddLink(1312, 3, 2, "on", 22, 14);
             links.AddLink(9213, 82, 3, "on", 22, 14);*/
            // links.RemoveLink(1115);
            //  links.ChangeLinkStatus(1112, "off");

            XMLParser test = new XMLParser("nodes.xml");
            //test.GetNodePorts(2);

            XMLParser test1 = new XMLParser("links.xml");
            //  test1.GetLinks();




            List<Link> linksList = test1.GetLinks();
            List<Node> nodesList = new List<Node>();
            nodesList.Add(new Node(81));
            nodesList.Add(new Node(82));
            nodesList.Add(new Node(83));
            nodesList.Add(new Node(84));
            nodesList.Add(new Node(1));
            nodesList.Add(new Node(2));
            nodesList.Add(new Node(3));
            nodesList.Add(new Node(4));
            nodesList.Add(new Node(5));
            nodesList.Add(new Node(6));
            nodesList.Add(new Node(7));
            nodesList.Add(new Node(8));
            nodesList.Add(new Node(9));
            nodesList.Add(new Node(10));
            nodesList.Add(new Node(11));

            // PathAlgorithm.dijkstra(nodesList, linksList, 81, 83, false);
            Console.WriteLine("qqqqqqqqqqqqqqq");
            PathAlgorithm.dijkstra(nodesList, linksList, 81, 84, false);
            Console.WriteLine("qqqqqqqqqqqqqqq");

            PathAlgorithm.dijkstra(nodesList, linksList, 82, 83, false);


            for (int i = 0; i < 14; i++)
            {
                Console.WriteLine(linksList[2].usedSlots[i]);
            }

            Console.Read();
        }
    }
}


/*
 *   XMLeonSubnetwork test = new XMLeonSubnetwork("test.xml","client.xml","links.xml");

            List<int> links = new List<int>();
            List<int> nodes = new List<int>();
            List<int> links1 = new List<int>();
            List<int> nodes1 = new List<int>();
            links.Add(1);
            links.Add(3);
            nodes.Add(4);

            test.AddSubnetwork(3,"11111", "linkxd");
            test.AddSubnetwork(2, "1.1.1.1",links1 ,nodes1);
            test.AddSubSubNetwork(2,4, "3.3.3", links, nodes);
            test.AddSubnetwork(3, "33333", links, nodes);

            test.GetSubnetworks();
*/
