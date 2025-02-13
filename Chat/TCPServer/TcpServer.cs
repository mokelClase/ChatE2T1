using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ChatLibrary;
using System.Reflection;

namespace TcpServer
{
    public class MyTcpMultipleListener
    {
        private List<User> userLista = new List<User>();
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
            new UserColor(10, 128, 0, 32),    // Borgoña
            new UserColor(11, 142, 69, 133),  // Ciruela
            new UserColor(12, 255, 0, 128),   // Fucsia
            new UserColor(13, 235, 99, 84),   // Rojo coral
            new UserColor(14, 139, 69, 19),   // Marrón café
            new UserColor(15, 128, 128, 128), // Gris
        ];

        // Klasearen atributuak.

        // Socket Listener.
        TcpListener server;


        // Eraikitzaile hutsa.
        public MyTcpMultipleListener(IPAddress ip, int port)
        {
            // TcpListener objektua sortzen dugu.
            this.server = new TcpListener(ip, port);

        }

        private void EntzutenHasi()
        {
            try
            {
                // Intentamos iniciar el servidor.
                this.server.Start();
                Console.WriteLine("Esperando conexiones...");

                // Bucle infinito para aceptar conexiones.
                int bezeroZenbakia = 0;
                while (true)
                {
                    // Espera una solicitud de conexión.
                    TcpClient socketcliente = this.server.AcceptTcpClient();
                    Console.WriteLine("Un cliente está intentando conectarse.");

                    // Gestionamos la solicitud del cliente en un hilo independiente.
                    Task.Run(() => this.BezeraKudeatu(socketcliente, bezeroZenbakia));
                }
            }
            catch (SocketException se)
            {
                // Si el puerto está en uso, manejamos el error y cerramos la aplicación.
                Console.WriteLine("El puerto ya está en uso. El servidor no pudo iniciarse.");
                Console.WriteLine("Error: " + se.Message);

                // Terminar inmediatamente el proceso
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception e)
            {
                // Cualquier otro error que no sea del socket
                Console.WriteLine("Error al iniciar el servidor: {0}", e.Message);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        /**
         * Bezerotik jasotako informazioa irakurri <EOF> jaso arte.
         * Ondoren, bezeroari jasotako mezua letra larriekin bueltatu.
         */
        private void BezeraKudeatu(TcpClient socket, int bezeroZenbakia)
        {
            // Stream-a ateratzen dugu.
            NetworkStream stream = socket.GetStream();
            // StreamReader eta StreamWriter objektuak datuak era eroso baten bidaltzen usten digu, Kontsolatik idazten egongo bagenu bezala.
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);

            // Bezeroak bidalitako informazioa hemen gortzen joango gara.
            string data = string.Empty;
            Boolean disconnection = false;
            Boolean userNoValido = false;
            try
            {
                // <EOF> jasotzen ez dugun bitartean, datuak irakurri.
                while (!disconnection && !userNoValido)
                {
                    string userName = "";
                    data += reader.ReadLine();
                    Console.WriteLine("Datos recibidos: " + data);
                    String[] dataArray = data.Split('_');
                    String codigoOperacion = dataArray[0];
                    // Nueva conexión
                    switch (codigoOperacion)
                    {
                        case "#newConnection":
                            Console.WriteLine("New Connection detectado");
                            userName = string.Join("_", dataArray.Skip(1));
                            if (userLista.Count > 0)
                            {
                                for (int i = 0; i < userLista.Count; i++)
                                {
                                    Console.WriteLine(userLista[i].Izena);
                                    userNoValido = (userLista[i].Izena == userName || userLista.Count >= 15);
                                }
                            }
                            if (!userNoValido)
                            {
                                bezeroZenbakia++;
                                int c = 0;
                                bool colorFound = false;
                                while (c < colorList.Count && colorFound)
                                {
                                    int b = 0;
                                    while (b < userLista.Count && !colorFound)
                                    {
                                        colorFound = (userLista[b].UserColor.Id == colorList[c].Id);
                                        b++;
                                    }
                                    c++;
                                }
                                Console.WriteLine("Color asignado: " + colorList[c].R + " " + colorList[c].G + " " + colorList[c].B);
                                User newUser = new User(userName, colorList[c]);
                                userLista.Add(newUser);
                                Console.WriteLine("El usuario " + userName + " ha sido añadido a la lista de usuarios");
                                Console.WriteLine("#connectionSuccesful_" + newUser.UserColor.Id + "_" + newUser.UserColor.R + "_" + newUser.UserColor.G + "_" + newUser.UserColor.B);

                                writer.WriteLine("#connectionSuccesful_" + newUser.UserColor.Id + "_" + newUser.UserColor.R + "_" + newUser.UserColor.G + "_" + newUser.UserColor.B);
                                writer.Flush();
                            }
                            else
                            {
                                Console.WriteLine("ERROR: El usuario " + userName + " no ha podido ser añadido a la lista de usuarios");
                                writer.WriteLine("#connectionFailed");
                                writer.Flush();
                            }
                            break;
                        case "#newMessage":
                            Console.WriteLine("New message detectado");
                            break;
                        case "#disConnection":
                            Console.WriteLine("Disconnection detectado");
                            userName = string.Join("_", dataArray.Skip(1));
                            bool found = false;

                            for (int i = 0; i < userLista.Count; i++)
                            {
                                Console.WriteLine("index: " + i + " Count: " + userLista.Count);
                                Console.WriteLine("izen actual: " + userLista[i].Izena + " izen buscado: " + userName);
                                if (userLista[i].Izena == userName)
                                {
                                    userLista.RemoveAt(i);
                                    found = true;
                                }
                            }

                            if (found)
                            {
                                writer.WriteLine("#operationSuccesful");
                                writer.Flush();
                                Console.WriteLine($"{userName} se ha desconectado.");
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
                            Console.WriteLine("ERROR: No se reconoce el código de la operación: " + codigoOperacion);
                            writer.WriteLine("#operationFailed");  // Responder siempre al cliente
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

            // Itxi konexioak.
            writer.Close();
            reader.Close();
            stream.Close();
            Console.WriteLine("Bezero-" + bezeroZenbakia + " konexioa itxita.");
        }

        /**
         * Irekitako konexio objektuak itxi.
         */
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

        /**
         * Main metodoa, programa hemen hasten da.
         */
        public static int Main(string[] args)
        {
            int port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            MyTcpMultipleListener zerbitzariAplikazioa = new MyTcpMultipleListener(localAddr, port);
            Console.WriteLine("Servidor iniciando...");

            zerbitzariAplikazioa.EntzutenHasi();  // Asegurar que empieza a escuchar conexiones

            Console.WriteLine("\nPresiona <ENTER> para finalizar...");
            Console.Read();
            return 0;
        }

    }
}