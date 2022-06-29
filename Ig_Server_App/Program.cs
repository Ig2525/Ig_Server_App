using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerApp
{
    internal class Program
    {
        //Для блокування потоку в процесі потоку
        static readonly object _lock = new object();
        //Список клієнтів, які підключаються на сервер
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            string fileName = "settings.txt"; //файл з налаштуваннями
            int count = 1;//Номер першого клієнта
            IPAddress ip;//ір адреса сервера
            int port;// порт на якому працює сервер
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))// читання файлу
                {
                    ip = IPAddress.Parse(sr.ReadLine());    //читаємо ір
                    port = int.Parse(sr.ReadLine());          //читаємо порт
                }
            }
            // створили сокет для сервера вказали кінцеву точку (ip i port)
            TcpListener serverSocket = new TcpListener(ip, port);
            serverSocket.Start();
            while (true)
            {
                //запит від клієнта прийшов
                TcpClient client = serverSocket.AcceptTcpClient();  //отримали клієнта
                lock (_lock) list_clients.Add(count, client); //клієнта додає у список
                Thread t = new Thread(handle_client);       //в окремому потоці обробляємо клієнта
                t.Start(count);                             //запускаємо потік і вказуємо номер клієнта
                count++;                                    //збільшуємо номер клієнта на 1
            }
        }
        //Обробка запиту клієнта
        private static void handle_client(object o)
        {
            int id = (int)o; //Зберегти номер клієнта
            TcpClient client = list_clients[id];//
            while (true)//цикл для роботи сокета клієнта
            {
                NetworkStream stream = client.GetStream();//Потік з даними клієнта
                Console.WriteLine("Client endpoint: " + client.Client.RemoteEndPoint);//
                byte[] buffer = new byte[1024];//Дані про клієнта
                int byte_count = stream.Read(buffer, 0, buffer.Length);//Читаємо дані клієнта
                if (byte_count == 0)// якщо дані відсутні,
                {
                    break;// то завершуємо цикл і спілкування із клієнтом
                }
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count);//
                broacast(data);// Розсилаємо повідомлення всім клієнтам, що є в чаті
                Console.WriteLine(data);// На сервер виводимо повідомлення клієгта
            }
            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        //Розсилка відповіді усім клієнтам
        private static void broacast(string data) //Повідомлення
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);// Текст перетворюємо в байти
            lock (_lock)// Блокуємо потік
            {
                foreach (TcpClient c in list_clients.Values)// розсилаємо усім хто є в чаті
                {
                    NetworkStream stream = c.GetStream();// отримали ссилку на клієнта
                    stream.Write(buffer, 0, buffer.Length);//відправили клієнту
                }
            }
        }
    }
}
