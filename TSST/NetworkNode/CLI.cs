using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// CLI programu
    /// </summary>
    class CLI
    {
        /// <summary>
        /// Wysłanie managerowi potwierdzenia, że zrobiło się wszystko tak jak chciał
        /// </summary>
        public static readonly string confirmation = "Zrobiłem wszystko tak jak chciałeś i wysyłam potwierdzenie";

        /// <summary>
        /// Wysłanie managerowi errora jak nie zadziała
        /// </summary>
        public static readonly string error = "Nie udało mi się zrobić wszystkiego tak jak chciałeś i wysyłam errora";

        /// <summary>
        /// Wypisuje błąd podczas wysyłania
        /// </summary>
        public static void PrintError()
        {
            Console.WriteLine("Błąd podczas wysyłania");
        }

        /// <summary>
        /// Printuje treść otrzymanej wiadomości
        /// </summary>
        /// <param name="message">treść wiadomości</param>
        /// <param name="port">numer portu</param>
        public static void PrintReceivedMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now} {message}");
        }

        internal static void ConnectedAgent()
        {
            Console.WriteLine("Połączono agenta");
        }

        /// <summary>
        /// Printuje wysłanie wiadomości
        /// </summary>
        /// <param name="message">treść wiadomości</param>
        /// <param name="port">numer portu</param>
        public static void PrintSentMessage(string port)
        {
            Console.WriteLine($"Wysyłam wiadomość na port {port}\n");
        }

        /// <summary>
        /// Informacja o odebraniu XML od managera
        /// </summary>
        /// <param name="name"></param>
        public static void PrintReceivedXML(string name)
        {
            Console.WriteLine($"{DateTime.Now} Otrzymano plik XML: {name}");
        }
    }
}