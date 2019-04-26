using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace NetworkNode
{
    //zbior funkcji
    //setMask
    //GetPort
    //SwapPort
    //GetAddress
    //GetLabel
    //SetLabelId
    //SetLabel
    //SetLabelS
    //SetTTL
    //DecreaseTTL
    //SetTC
    //CehckTTL
    //Push
    //Pop
    //Swap
    // MultiplePush
    //



    /// <summary>
    /// klasa do obslugi wszystkich operacji na etykietach
    /// zrodlo-https://www.juniper.net/documentation/en_US/junos/topics/concept/mpls-labels-operations.html
    /// @author Pawel
    /// </summary>
    class Label
    {
        public uint IDswap;
        public uint IDpush;
        public uint labelS;
        public uint TTLswap;
        public uint TTLpush;
        public uint labelTC;
        public int portOut;
        public uint labelOut;
        public string action;
        public string address;


        public Label(uint IDswap, uint IDpush, string action, int portOut)
        {
            this.IDpush = IDpush;
            this.IDswap = IDswap;
            this.action = action;
            this.portOut = portOut;
        }
        public Label(string address, uint IDpush, string action, int portOut)
        {
            this.IDpush = IDpush;
            this.address = address;
            this.action = action;
            this.portOut = portOut;
        }


        public static uint label;
        /// <summary>
        /// parametry labela, zgodne z wiki
        /// </summary>
        public static uint ID;
        public static uint S;
        public static uint TTL;
        public static uint TC;

        /// <summary>
        /// maski labela tak, by wycinac z label odpowiednie informacje
        /// </summary>
        static uint maskID;
        static uint maskTTL;
        static uint maskTC;
        static uint maskS;

        /// <summary>
        /// anty maski sa po to, by resetowac odpowiednie fragmenty w labelu
        /// </summary>
        static uint antiMaskID;
        static uint antiMaskTTL;
        static uint antiMaskTC;
        static uint antiMaskS;

        /// <summary>
        /// deklaracja masek, musi byc wywolana przed uruchomieniem operacji na nich
        /// </summary>
        public static void setMask()
        {
            maskID = (uint)Math.Pow(2, 20) - 1;
            maskTTL = (uint)Math.Pow(2, 32) - (uint)Math.Pow(2, 24);
            maskTC = (uint)Math.Pow(2, 23) - (uint)Math.Pow(2, 20);
            maskS = (uint)Math.Pow(2, 23);

            antiMaskID = maskTTL + maskTC + maskS;
            antiMaskS = maskID + maskTTL + maskTC;
            antiMaskTTL = maskID + maskTC + maskS;
            antiMaskTC = maskID + maskTTL + maskS;

            long a = maskTTL + maskTC + maskS + maskID;
            //if (maskTTL + maskTC + maskS + maskID == 4294967295)
            //Console.WriteLine("good");
        }

        /// <summary>
        /// wyciaga informacje na ktory port przyszla wiadomosc
        /// </summary>
        /// <param name="message"></param>
        public static int GetPort(string message)
        {
            try
            {
                int start_port = message.IndexOf("<port>") + 6;
                int end_port = message.IndexOf("</port>");
                int port = Int32.Parse(message.Substring(start_port, end_port - start_port));

               // Console.WriteLine("  Znaleziony port: " + port);

                return port;
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Nie znalazłem portu");
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// bierze z wiadomosci pierwszy slot na ktorym wysylamy
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int GetStartSlot(string message)
        {

            try
            {
                int start_slot = message.IndexOf("<start_slot>") + 12;
                int end = message.IndexOf("</start_slot>");
                int slot = Int32.Parse(message.Substring(start_slot, end - start_slot));
                Console.WriteLine("  Slot startowy: " + slot);
                return slot;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie znalazłem start_slot");
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }


        /// <summary>
        /// zmienia informacje na jaki nowy port ma isc
        /// </summary>
        /// <param name="message"></param>
        /// <param name="newPort"></param>
        /// <returns></returns>
        public static string SwapPort(string message, int newPort)
        {
            int startPort = message.IndexOf("<port>") + 6;
            int endPort = message.IndexOf("</port>");

            message = message.Remove(startPort, endPort - startPort);
            string stringLabel = newPort.ToString();
            message = message.Insert(startPort, stringLabel);
            // Console.WriteLine("SwapPort: " + message);
            // Console.WriteLine("  Został zamieniony port: " + message);
            Console.WriteLine("  Port wejsciowy zostal zamieniony na port: " +stringLabel);

            return message;
        }

        public static string GetAddress(string message)
        {
            string address;

            try
            {
                int start_address = message.IndexOf("<address>") + 9;
                int end_address = message.IndexOf("</address>");
                address = (message.Substring(start_address, end_address - start_address));

                Console.WriteLine("Znaleziony adres: " + address);

                //Console.WriteLine("znaleziony address: "+address);
                return address;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nie mogę znaleźć adresu " + ex.ToString());
                return null;

            }

        }

        /// <summary>
        /// pozyskuje dane z wiadomosci, ktore zapisuje w zmiennych statycznych
        /// 
        /// ID-etykieta
        /// TC-dla QoS
        /// S-jak 1 to oznacza, ze jest ostatnia na stosie
        /// TTL-time to live

        /// </summary>
        /// <param name="message"></param>
        public static uint GetLabel(string message)
        {
            try
            {
                int start_label = message.IndexOf("<label>") + 7;
                int end_label = message.IndexOf("</label>");
                label = UInt32.Parse(message.Substring(start_label, end_label - start_label));

                S = (label & maskS) / 8388608;
                TTL = (label & maskTTL) / 16777216;
                TC = (label & maskTC) / 1048576;
                ID = label & maskID;
                Console.WriteLine($"Etykieta: {label}");
                Console.WriteLine($"ID: {ID} TC: {TC} S: {S} TTL: {TTL}");
                return label;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// ustala nowy ID
        /// </summary>
        /// <param name="newID"></param>
        public static void SetLabelID(uint newID)
        {
            if (newID > 1048575 || newID < 0)
            {
                throw new InvalidOptionException("Bad new Label ID");
            }
            //stara wersja działa, wiec niech jeszcze tu bedzie
            /*byte[] bytes = BitConverter.GetBytes(label);
            byte[] bytesEnd = BitConverter.GetBytes(newID);
            for(int i=0;i<3;i++)
                 bytes[i] = bytesEnd[i];

            uint help = UInt32.Parse(bytes[2].ToString())+TC*16+S*128;
            byte[] bytehelp= BitConverter.GetBytes(help);
            bytes[2] = bytehelp[0];
            label = BitConverter.ToUInt32(bytes,0);*/
            //nowa wersja
            label &= antiMaskID;
            label += newID;
        }

        /// <summary>
        /// ustala nowa etykiete
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TC"></param>
        /// <param name="S"></param>
        /// <param name="TTL"></param>
        public static void SetLabel(uint ID, uint TC, uint S, uint TTL)
        {
            if (ID > 1048575 || ID < 0)
            {
                throw new InvalidOptionException("Bad new Label ID");
            }
            if (S > 1 || S < 0)
            {
                throw new InvalidOptionException("Bad new Label S");
            }
            if (TC > 7 || TC < 0)
            {
                throw new InvalidOptionException("Bad new Label TC");
            }
            if (TTL > 255 || TTL < 0)
            {
                throw new InvalidOptionException("Bad new Label TTL");
            }
            label = ID + TC * (uint)Math.Pow(2.0, 20) + S * (uint)Math.Pow(2.0, 23) + (TTL * (uint)Math.Pow(2.0, 24));
        }

        /// <summary>
        /// ustala dla labela tego statycznego, czy jest na dole, czy gorze
        /// 
        /// </summary>
        /// <param name="stan"></param>
        public static void SetLabelS(uint stan)
        {
            if (S > 1 || S < 0)
            {
                throw new InvalidOptionException("Bad new SetLabel S");
            }
            //uint i = 4294967295-8388608;// 42866578687;
            S = stan;
            if (stan == 1)
                label = label | (1 << 23);
            if (stan == 0)
                label &= antiMaskS;//(1048576+1);
        }



        /// <summary>
        /// ustala time to live dla static labela
        /// </summary>
        /// <param name="time"></param>
        public static void SetTTL(uint time)
        {
            if (time > 255)
            {
                throw new InvalidOptionException("Bad new SetTTL Label time");
            }
            label &= antiMaskTTL;
            label += (uint)Math.Pow(2, 24) * time;
        }



        /// <summary>
        /// zmniejsza czas zycia labela
        /// </summary>
        public static void DecreaseTTL()
        {
            if (TTL > 255 || TTL < 0)
            {
                throw new InvalidOptionException("Bad new Label TTL");
            }

            label -= (uint)Math.Pow(2, 24);
        }


        /// <summary>
        /// ustala TC dla danego static labela
        /// </summary>
        /// <param name="tc"></param>
        public static void SetTC(uint tc)
        {
            if (tc > 7)
            {
                throw new InvalidOptionException("Bad new Label TTL");
            }
            label &= antiMaskTC;
            label += (uint)Math.Pow(2, 20) * tc;
        }


        /// <summary>
        /// sprawdza czy pakiet ma jeszcze prawo zyc
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool CheckTTL()
        {
            if (TTL > 0)
                return true;
            else
                return false;
        }


        /// <summary>
        /// dodaje etykiete na gore stosu
        /// </summary>
        public static string Push(string message, uint newLabel)
        {
            int indeks = message.IndexOf("</port>") + 7;

            string stringLabel = "<label>" + newLabel + "</label>";
            message = message.Insert(indeks, stringLabel);
            Console.WriteLine("Dodaje etykiete: " + newLabel);
            return message;
        }



        /// <summary>
        /// zdejmuje gorna etykiete
        /// </summary>
        public static string Pop(string message)
        {
            int start_label = message.IndexOf("<label>");
            int end_label = message.IndexOf("</label>");
            Console.WriteLine("Usuwam etykiete: " +
                message.Substring(message.IndexOf("<label>") + 7, message.IndexOf("</label>") - message.IndexOf("<label>") - 7));
            message = message.Remove(start_label, end_label - start_label + 8);
            return message;
        }



        /// <summary>
        /// zamienia gorna etykiete na stosie
        /// nie jest uzywany replace, poniewaz jak bysmy mieli dwa labele
        /// o tej samej wartosci to zamienilby oba
        /// </summary>
        public static string Swap(string message, uint newLabel)
        {
            //message = Label.pop(message);
            //message = Label.push(message,newLabel);
            int start_label = message.IndexOf("<label>") + 7;
            int end_label = message.IndexOf("</label>");
            var removedLabel = message.Substring(message.IndexOf("<label>") + 7, message.IndexOf("</label>") - message.IndexOf("<label>") - 7);
            message = message.Remove(start_label, end_label - start_label);

            string stringLabel = newLabel.ToString();
            message = message.Insert(start_label, stringLabel);
            Console.WriteLine("Zmieniam etykiete: " + removedLabel + " na: " +
                message.Substring(message.IndexOf("<label>") + 7, message.IndexOf("</label>") - message.IndexOf("<label>") - 7));

            return message;
        }

        /// <summary>
        /// Ustawia path na pierwszym routerze
        /// </summary>
        /// <param name="message">wiadomość</param>
        /// <param name="number">numer routera</param>
        /// <returns></returns>
        public static string SetPath(string message, int number)
        {
            return message += ("<path>" + number + "</path>");
        }

        /// <summary>
        /// Zwraca ścieżke
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetPath(string message)
        {
            var start_path = message.IndexOf("<path>") + 6;
            var end_path = message.IndexOf("</path>");

            return message.Substring(message.IndexOf("<path>") + 6,
                message.IndexOf("</path>") - message.IndexOf("<path>") - 6);
        }

        public static string AddToPath(string message, int number)
        {
            var path = GetPath(message);
            path += number;

            var start_path = message.IndexOf("<path>") + 6;
            var end_path = message.IndexOf("</path>");

            message = message.Remove(start_path, end_path - start_path);
            message = message.Insert(start_path, path);
            return message;
        }

        /// <summary>
        /// dodawanie wielu etykiet do pakietu, max 3, ta operacja rowna sie parokrotnemu pushowaniu
        /// </summary>
        public void MultiplePush()
        {
        }
        /// <summary>
        /// zamienia gorna etykiete a nastepnie dodaje nowa na wierzch
        /// </summary>
        public void SwapAndPush()
        {
        }


    }

    [Serializable]
    internal class InvalidOptionException : Exception
    {
        public InvalidOptionException()
        {
        }

        public InvalidOptionException(string message) : base(message)
        {
        }

        public InvalidOptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidOptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
