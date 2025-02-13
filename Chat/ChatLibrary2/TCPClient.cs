using System;
using System.Collections.Generic;
using System.IO;
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

        // Evento para actualizar la lista de usuarios en la UI
        public event Action<List<User>> OnUserListUpdated;

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

        private void ProcesarMensaje(string mensaje)
        {
            if (mensaje.StartsWith("#updateUserList_"))
            {
                ActualizarListaUsuarios(mensaje);
            }
            else if (mensaje.StartsWith("#message_"))
            {
                Console.WriteLine("Nuevo mensaje: " + mensaje);
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

        private void ActualizarListaUsuarios(string mensaje)
        {
            List<User> usuarios = new List<User>();
            string[] partes = mensaje.Split('_');
            if (partes.Length > 1)
            {
                for (int i = 1; i < partes.Length; i++)
                {
                    usuarios.Add(new User(partes[i]));
                }
            }
            OnUserListUpdated?.Invoke(usuarios);
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
                    Console.WriteLine("Error al desconectar: {0}", e.Message);
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
                Console.WriteLine("Conexión cerrada.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error cerrando conexión: {0}", e.Message);
            }
        }
    }
}
