using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using ChatLibrary;

namespace TcpServer
{
    public class MyTcpMultipleListener
    {
        private List<User> userLista = new List<User>();
        private List<Msg> msgLista = new List<Msg>();
        private List<StreamWriter> writersList = new List<StreamWriter>();
        private List<UserColor> colorList = [
            new UserColor(1, 255, 0, 0),     // Rojo brillante  
            new UserColor(2, 255, 140, 0),   // Naranja intenso
            new UserColor(3, 218, 165, 32),  // Dorado
            new UserColor(4, 80, 200, 120),  // Verde esmeralda
            new UserColor(5, 0, 138, 0),     // Verde oscuro
            new UserColor(6, 0, 206, 209),   // Turquesa oscuro
            new UserColor(7, 0, 162, 232),   // Azulón
            new UserColor(8, 0, 0, 255),     // Azul oscuro
            new UserColor(9, 138, 43, 226),  // Violeta fuerte
            new UserColor(10, 128, 0, 32),   // Borgoña
            new UserColor(11, 142, 69, 133), // Ciruela
            new UserColor(12, 255, 0, 128),  // Fucsia
            new UserColor(13, 235, 99, 84),  // Rojo coral
            new UserColor(14, 139, 69, 19),  // Marrón café
            new UserColor(15, 128, 128, 128) // Gris
        ];

        private TcpListener server;

        // Constructor
        public MyTcpMultipleListener(IPAddress ip, int port)
        {
            this.server = new TcpListener(ip, port);
        }

        private void EntzutenHasi()
        {
            try
            {
                // Inicia el servidor
                this.server.Start();
                Console.WriteLine("Konexioak itxaroten...");

                int bezeroZenbakia = 0;
                while (true)
                {
                    // Espera una conexión
                    TcpClient socketcliente = this.server.AcceptTcpClient();
                    Console.WriteLine("Bezero bat konektatzen saiatzen ari da.");

                    // Procesa la conexión en un hilo separado
                    Task.Run(() => this.BezeraKudeatu(socketcliente, bezeroZenbakia));
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("Port-a erabiltzen ari da. Server-a ezin da hasi.");
                Console.WriteLine("Error: " + se.Message);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine("Zerbitzari hastean errorea: {0}", e.Message);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void BezeraKudeatu(TcpClient socket, int bezeroZenbakia)
        {
            NetworkStream stream = socket.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);

            // Añadir el StreamWriter de este cliente a la lista
            writersList.Add(writer);

            string data = string.Empty;
            Boolean disconnection = false;
            Boolean userNoValido = false;
            try
            {
                while (!disconnection && !userNoValido)
                {
                    string userName = "";
                    data += reader.ReadLine();
                    Console.WriteLine("Jasotako datuak: " + data);
                    String[] dataArray = data.Split('_');
                    String codigoOperacion = dataArray[0];

                    switch (codigoOperacion)
                    {
                        case "#newConnection":
                            Console.WriteLine("New Connection aurkitu da");
                            userName = string.Join("_", dataArray.Skip(1));

                            if (userLista.Count > 0)
                            {
                                for (int i = 0; i < userLista.Count; i++)
                                {
                                    userNoValido = (userLista[i].Izena == userName || userLista.Count >= 15);
                                }
                            }

                            if (!userNoValido)
                            {
                                bezeroZenbakia++;
                                int c = 0;
                                bool colorLibre = false;

                                if (userLista.Count() >= 1)
                                {
                                    while (c < colorList.Count && !colorLibre)
                                    {
                                        int b = 0;
                                        while (b < userLista.Count() && (userLista[b].UserColor.Id != colorList[c].Id))
                                        {
                                            b++;
                                        }
                                        if (b == userLista.Count())
                                        {
                                            colorLibre = true;
                                        }
                                        else
                                        {
                                            c++;
                                        }
                                    }
                                }

                                User newUser = new User(userName, colorList[c]);
                                userLista.Add(newUser);
                                Console.WriteLine("Erabiltzaile " + userName + " usuario listan sartu da");

                                writer.WriteLine("#connectionSuccesful_" + newUser.UserColor.Id + "_" + newUser.UserColor.R + "_" + newUser.UserColor.G + "_" + newUser.UserColor.B);
                                writer.Flush();

                                foreach (var mensaje in msgLista)
                                {
                                    Console.WriteLine("Mezu zahar: #newMessage_" + mensaje.ToString());
                                    writer.WriteLine("#newMessage_" + mensaje.ToString());
                                    writer.Flush();
                                }
                            }

                            else
                            {
                                Console.WriteLine("ERROR: Erabiltzaile " + userName + " ezin da erabiltzaile listan sartu");
                                writer.WriteLine("#connectionFailed");
                                writer.Flush();
                            }
                            break;

                        case "#newMessage":
                            Console.WriteLine("New message aurkitu da");
                            string mezuText = dataArray[1];
                            string senderName = string.Join("_", dataArray.Skip(3));
                            User sender = new User(senderName);

                            foreach (var user in userLista)
                            {
                                if (user.Izena == senderName)
                                {
                                    sender = user;
                                }
                            }

                            Msg mezu = new Msg(sender, mezuText, DateTime.Now);
                            msgLista.Add(mezu);
                            Console.WriteLine("Enviando: #newMessage_" + mezu.ToString());

                            // Enviar el mensaje a todos los clientes
                            foreach (var clientWriter in writersList)
                            {
                                clientWriter.WriteLine("#newMessage_" + mezu.ToString());
                                clientWriter.Flush();
                            }
                            break;

                        case "#disConnection":
                            Console.WriteLine("Disconnection aurkitu da");
                            userName = string.Join("_", dataArray.Skip(1));
                            bool found = false;

                            for (int i = 0; i < userLista.Count; i++)
                            {
                                if (userLista[i].Izena == userName)
                                {
                                    userLista.RemoveAt(i);
                                    found = true;
                                }
                            }

                            if (found)
                            {
                                Console.WriteLine($"{userName} deskonektatu da.");
                                writersList.Remove(writer); // Elimina el StreamWriter de la lista
                                writer.Close();
                                reader.Close();
                                socket.Close();
                                return;
                            }
                            else
                            {
                                writer.WriteLine("#operationFailed");
                                writer.Flush();
                            }
                            break;

                        default:
                            Console.WriteLine("ERROR: Operaketaren kodea ez da aurkitzen: " + codigoOperacion);
                            writer.WriteLine("#operationFailed");
                            writer.Flush();
                            break;
                    }

                    data = "";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Komunikazio errorea: {0}", e);
            }

            // Cerrar la conexión
            writer.Close();
            reader.Close();
            stream.Close();
            Console.WriteLine("Bezero-" + bezeroZenbakia + " konexioa itxita.");
        }

        private void Itxi()
        {
            try
            {
                this.server.Stop();
                Console.WriteLine("Zerbitzaria bukatuta.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Zerbitzaria ezin izan da gelditu: {0}", e);
            }
        }

        public static int Main(string[] args)
        {
            int port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            MyTcpMultipleListener zerbitzariAplikazioa = new MyTcpMultipleListener(localAddr, port);
            Console.WriteLine("Zerbitzaria hasten...");
            zerbitzariAplikazioa.EntzutenHasi();

            return 0;
        }
    }
}
