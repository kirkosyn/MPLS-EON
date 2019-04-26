using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientNode
{
    class CLI
    {
        internal static void ConnectedAgent()
        {
            Console.WriteLine("Połączono agenta");
        }

        /// <summary>
        /// polecenia i instrukcja obslugi klijenta
        /// </summary>
        internal static void Promt()
        {
            Console.WriteLine();
            Console.WriteLine("KOMENDY:");
            //Console.WriteLine("zestaw polaczenie,po dwukropku id odbiorcy://connection:");
            Console.WriteLine("Podaj id klienta, z którym ma być zestawione połączenie: //connection:");
            //Console.WriteLine("wybierz odbiorce, po dwukropku numer odbiorcy://client:");
            Console.WriteLine("Podaj id klienta, do którego ma przyjść wiadomość, po dwukropku numer odbiorcy: //client:");
            Console.WriteLine("po wybraniu odbiorcy pisany tekst jest wiadomością do przesłania.");
            Console.WriteLine("Usuń połączenie, po dwukropku numer połączenia: //delete:");
            //Console.WriteLine("spamuj odbiorce wpisz://send  teraz kliknij enter i wiadomosc ktora chcesz spamowac");
            Console.WriteLine("Wyślij ciąg 10 wiadomości do wybranego klienta: //send");
            Console.WriteLine();

        }

        /// <summary>
        /// przelaczanie polecen w zaleznosci co wybierze user i wywolywanie wtedy odpowiednich akcji
        /// </summary>
        /// <param name="clientNode"></param>
        internal static void SwitchCommands(Port clientNode)
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message.Contains("//client:"))
                {
                    try
                    {
                        clientNode.client = Int32.Parse(message.Substring(9));
                        Console.WriteLine("Odbiorcą jest klient: " + clientNode.client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Zła komenda, spróbuj ponownie.");
                    }

                }
                else if (message.Contains("//send"))
                {
                    message = Console.ReadLine();
                    for (int i = 0; i < 10; i++)
                    {
                        clientNode.Send(message, clientNode.client);
                        Thread.Sleep(500);
                    }
                }
                else if (message.Contains("//connection:"))
                {
                    try
                    {

                        int connection = Int32.Parse(message.Substring(13));
                        Console.WriteLine("Proszę o zestawienie połączenia z klientem: " + connection);

                        lock (Agent.agentCollection)
                        {
                            // Console.WriteLine("aa");
                            Agent.agentCollection.Add("//connection:<target_client>" + connection + "</target_client>");
                            //Console.WriteLine("baa");

                        }//clientNode.SendCommand(message);


                    }
                    catch
                    {
                        Console.WriteLine("Zła komenda, spróbuj ponownie.");
                    }
                }
                else if (message.Contains("//delete:"))
                {
                    try
                    {
                        int connection = Int32.Parse(message.Substring(9));

                        Agent.clientEonDictioinary.Remove(connection);

                        Console.WriteLine("Prosze o usunięcie połączenia z klientem: " + connection);

                        lock (Agent.agentCollection)
                        {
                            Agent.agentCollection.Add("//delete:<target_client>" + connection + "</target_client>");
                        }
                        //clientNode.SendCommand(message);
                    }
                    catch
                    {
                        Console.WriteLine("Zła komenda, spróbuj ponownie.");
                    }
                }
                //jezeli nie sa to polecenia konsolowe wysyla po prostu wiadomosc
                else
                {
                    clientNode.Send(message, clientNode.client);
                }
            }
        }
    }
}
