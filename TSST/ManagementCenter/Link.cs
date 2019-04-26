using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    /// <summary>
    /// klasa sluzaca by przechowywac informacje o linkach
    /// co lacze, jak sa zajet itp
    /// taki substytut CLI
    /// </summary>
    class Link
    {
        public int nodeA;
        public int nodeB;

        public int id;
        public int cost;
        public int lenght;

        string status;

        public int numberOfSlots;

        public bool[] usedSlots;

        public int portIn;
        public int portOut;

        public Link()
        { }

        /// <summary>
        ///  jak jest w podsieci na brzegu, by dac go do brzegowego portu jako link wejsciowy lub wyjsciowy w zaleznosci od potrzeby
        /// </summary>
        public Link(int portCheat)
        {
            portIn = portCheat;
            portOut = portCheat;
        }

        public Link(int id, int nodeA, int nodeB, int numberOfSlots, int cost, string status, int lenght)
        {
            this.id = id;
            this.numberOfSlots = numberOfSlots;
            this.nodeA = nodeA;
            this.nodeB = nodeB;

            portIn = (nodeA + 10) * 100 + (nodeB + 10);
            portOut = (nodeB + 10) * 100 + (nodeA + 10);



            this.status = status;

            this.cost = cost;
            this.lenght = lenght;

            usedSlots = new bool[numberOfSlots];
            // usedSlots = Array.CreateInstance(typeof(bool), this.numberOfSlots);
            for (int i = 0; i < numberOfSlots; i++)
            {
                usedSlots.SetValue(false, i);
            }
        }

        /// <summary>
        /// rezeruwuje lub zwalnie oznaczenie czy szczelina jest zajeta
        /// </summary>
        /// <param name="startSlot"></param>
        /// <param name="endSlot"></param>
        /// <param name="status"></param>
        public void SetSlots(int startSlot, int endSlot, bool status)
        {
            //indeksowanie, sloty od 1, tablice od 0
            //lacznie z end slotem
            for (int i = startSlot - 1; i <= endSlot - 1; i++)
            {
                usedSlots.SetValue(status, i);
            }

        }
    }
}
