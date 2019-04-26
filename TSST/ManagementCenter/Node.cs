using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ManagementCenter
{
    /// <summary>
    /// informacje o wezlach sa tu przechowywane
    /// bardzo wazne, jako nody beda liczeni tez klijenci w algorytmie sciezek
    /// </summary>
    class Node : ICloneable
    {
        public int number;
        //Array ports;
        List<int> ports;

        public bool isAlive;

        //koszt dotaracia do tego wezla, potrzebna do dijxtry
        public int costToGetHere;

        //wezel ktory jest poprzedni na sciezce
        public int previousNode;

        //z ktorego linku wychodzi z wezla
        public Link outputLink;

        //z ktorego linku wchodzi do wezla
        public Link inputLink;

        //uzywany do obliczen, czy juz jest na sciezce
        public bool connected;

        public List<Tuple<int, int>> connections;



        public Node(int number, List<int> ports)
        {
            this.number = number;
            this.ports = ports;

            isAlive = true;
            connected = false;
            //maksymalny koszt jaki moze miec int
            costToGetHere = 2147483647;
        }

        public Node(int number)
        {
            this.number = number;

            isAlive = true;
            connected = false;
            //maksymalny koszt jaki moze miec int
            costToGetHere = 2147483647;
        }

        public Node(int number, Link outputLink, Link inputLink, List<int> ports, bool alive, int costToGet, int previousNode, bool connected)
        {
            this.number = number;
            this.outputLink = outputLink;
            this.inputLink = inputLink;
            this.ports = new List<int>();

            //tu moze byc zle liste kopiowana
            this.ports = ports;


            this.isAlive = alive;
            this.costToGetHere = costToGet;
            this.previousNode = previousNode;
            this.connected = connected;

        }

        public static void ResestConnectionStatus(List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                node.connected = false;
                node.costToGetHere = 2147483647;
            }
        }

        /// <summary>
        /// sluzy do klonowania obiektu
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Node(this.number, this.outputLink, this.inputLink, this.ports, this.isAlive, this.costToGetHere, this.previousNode, this.connected);
        }
    }
}
