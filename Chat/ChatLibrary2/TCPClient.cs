using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatLibrary
{
    public class TCPClient
    {
        private TcpClient client = null;
        private NetworkStream str = null;
        private StreamReader sr = null;
        private StreamWriter sw = null;
        private bool connected = false;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public User bezero;
        public List<Msg> msgLista= new List<Msg>();

        public TCPClient(string bezIzena)
        {
            this.bezero = new User(bezIzena);
            this.connected = this.Konektatu("127.0.0.1", 13000);

            if (connected)
            {
                EscucharServidorAsync(); // Iniciar escucha asincrónica
            }
        }

        private bool Konektatu(string server, int port)
        {
            try
            {
                this.client = new TcpClient(server, port);
                this.client.ReceiveTimeout = 5000;
                this.str = this.client.GetStream();
                this.sr = new StreamReader(this.str);
                this.sw = new StreamWriter(this.str);

                this.sw.WriteLine("#newConnection_" + bezero.Izena);
                this.sw.Flush();

                string response = this.sr.ReadLine();
                string[] dataArray = response.Split('_');

                if (dataArray[0] == "#connectionSuccesful")
                {
                    connected = true;
                    this.bezero.UserColor = new UserColor(int.Parse(dataArray[1]), int.Parse(dataArray[2]), int.Parse(dataArray[3]), int.Parse(dataArray[4]));
                    //getMessages();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al conectar: {0}", e.Message);
            }
            return false;
        }

        private async void EscucharServidorAsync()
        {
            try
            {
                while (connected)
                {
                    // Usamos ReadLineAsync para lectura asincrónica
                    string response = await sr.ReadLineAsync();

                    if (!string.IsNullOrEmpty(response))
                    {
                        ProcesarMensaje(response);
                    }
                }
            }
            catch (Exception e)
            {
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Error escuchando servidor: {0}", e.Message);
                }
            }
        }
        //public void getMessages()
        //{
        //    try
        //    {
        //        this.sw.WriteLine("#getMessages");
        //        this.sw.Flush();
        //        string answer=this.sr.ReadLine();
        //        string[] mezuArray=answer.Split('_');
        //        if (answer.StartsWith("#setMessages_"))
        //        {
        //            setMezuak(mezuArray);
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Errorea mezuak hartzen: " + e);
        //    }
        //}

        public void setMezuak(string[] data)
        {

        }

        public bool SendMsg(string mezu, Object controller)
        {
            try
            {
                this.sw.WriteLine("#newMessage_" + mezu + "_#userName_" + this.bezero.Izena);
                this.sw.Flush();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Errorea mezua bidaltzen: " + e);
                return false;
            }
        }
        private void ProcesarMensaje(string mensaje)
        {
            String[] dataArray = mensaje.Split('_');
            if (dataArray[0]=="#newMessage")
            {
                updateMezu(dataArray);
            }
            else if (mensaje == "#operationSuccesful")
            {
                Console.WriteLine("Operación exitosa.");
            }
            else
            {
                Console.WriteLine("Mensaje desconocido: " + mensaje);
            }
        }

        public void updateMezu(string[] data)
        {
            UserColor kolore = new UserColor(int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]));
            User sender = new User(data[2], kolore);
            Msg newMezu = new Msg(sender,data[1], data[6]);
            
        }

        public bool Disconnect()
        {
            if (connected)
            {
                try
                {
                    this.sw.WriteLine("#disConnection_" + bezero.Izena);
                    this.sw.Flush();
                    this.client.ReceiveTimeout = 2000;
                    string response = this.sr.ReadLine();

                    if (response == "#operationSuccesful")
                    {
                        connected = false;
                        Itxi();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Errorea deskonektatzen: {0}", e.Message);
                }
                finally
                {
                    Itxi();
                }
            }
            return false;
        }

        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        private void Itxi()
        {
            try
            {
                cancellationTokenSource.Cancel(); // Cancelar la escucha asincrónica
                sr?.Close();
                sw?.Close();
                str?.Close();
                client?.Close();
                Console.WriteLine("Konexioa ixten...");
            }
            catch (Exception e)
            {
                Console.WriteLine("Errorea konexioa ixten: {0}", e.Message);
            }
        }
    }
}
