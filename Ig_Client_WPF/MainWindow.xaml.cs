using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ig_Client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Опис змінних:
        TcpClient client = new TcpClient();// клієнт, який відправляє запити
        NetworkStream ns;//Потік для спілкування із сервером
        Thread thread;// Потік для отримання запитів від сервера
        private ChatMessage _message = new ChatMessage();//повідомлення , яке надсилається чи отримується

        //Ініціалізація
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = "settings.txt";   //файл з налаштуваннями
                IPAddress ip;                       //ір адреса сервера
                int port;                           // порт на якому працює сервер
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))  // читання файлу
                    {
                        ip = IPAddress.Parse(sr.ReadLine());        //читаємо ір
                        port = int.Parse(sr.ReadLine());            //читаємо порт
                    }
                }
                _message.UserName = txtUserName.Text;           // Вказуємо ім'я користувача
                _message.UserId = Guid.NewGuid().ToString();      //Вказуємо ідентифікатор
                client.Connect(ip, port);                       // конектимось до сервера

                //Повідомлкеея про успішне підкдючення
                lbInfo.Items.Add($"Підключаємось до сервера {ip.ToString()}:{port}");

                //Отримуємо потік даних від сервера
                ns = client.GetStream();

                //Запускаємо другорядний потік для отримання повідомлень від сервера
                thread = new Thread(o => ReceiveData((TcpClient)o));//
                thread.Start(client);                           // Запускаємо потік
                _message.MessageType = TypeMessage.Login;       // Виконуємо логін
                _message.Text = "Прєднався до чату";            // Текст повідомлення
                byte[] buffer = _message.Serialize();           // серіалізуємо повідомлення в байти

                //var deser = ChatMessage.Deserialize(buffer);
                ns.Write(buffer, 0, buffer.Length);             // відправляємо повідомлення на сервер
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem connect server: " + ex.Message);
            }
        }
        //Метод для отримання даних від сервера
        private void ReceiveData(TcpClient client)      //Ссилка на клієнта
        {
            NetworkStream ns = client.GetStream();      //Читаємо дані від сервера
            byte[] receivedBytes = new byte[1024];      //Зберігаємо дані які надіслав сервер
            int byte_count;                             //Розмір масиву
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)  //Читаємо повідомлення від сервера
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ChatMessage message = ChatMessage.Deserialize(receivedBytes);
                        switch (message.MessageType)
                        {
                            case TypeMessage.Login:
                                {
                                    if (message.UserId != _message.UserId)
                                        lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;
                                }
                            case TypeMessage.Logout:
                                {
                                    if (message.UserId != _message.UserId)
                                        lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;
                                }
                            case TypeMessage.Message:
                                {
                                    lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;
                                }
                        }
                        lbInfo.Items.MoveCurrentToLast();
                        lbInfo.ScrollIntoView(lbInfo.Items.CurrentItem);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }));
            }
        }
        //
        //подія закриття вікна
        private void Window_Closed(object sender, EventArgs e)
        {
            _message.MessageType = TypeMessage.Logout;  //Тип повідомлення вихід
            _message.Text = "Покинув чат";              //Текст повідомлення
            byte[] buffer = _message.Serialize();       // Серіалізуємо повідомлення  (перетворюємо) в байти
            ns.Write(buffer, 0, buffer.Length);         // Відправляємо повідомлення на сервер
            client.Client.Shutdown(SocketShutdown.Both);// Звершуємо роботу клієнта
            thread.Join();                              // Очікуємо завершення виконання задач у потоці
            ns.Close();                                 //Закриваємо потік з'єднаня із сервером
            client.Close();                             //Закриваємо клієнта
        }

        private void BntSend_Click(object sender, RoutedEventArgs e)
        {
            //Формуємо повідомлення 
            _message.MessageType = TypeMessage.Message; //Тип повідомлення текст
            _message.Text = txtText.Text;               //Текст повідомлення
            byte[] buffer = _message.Serialize();       //Повідомлення перетворюємо в байти
            ns.Write(buffer, 0, buffer.Length);         //Відправляємо повідомлення на сервер
        }

        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                imgAvatar.Source = new BitmapImage(fileUri);
            }



            // Configure open file dialog box
            //var dialog = new Microsoft.Win32.OpenFileDialog();
            //dialog.FileName = "Avatar"; // Default file name
            //dialog.DefaultExt = ".jpg"; // Default file extension
            //dialog.Filter = "Image (.jpg)|*.jpg"; // Filter files by extension

            //// Show open file dialog box
            //bool? result = dialog.ShowDialog();

            //// Process open file dialog box results
            //if (result == true)
            //{
            //    // Open document
            //    string filename = dialog.FileName;
            //}


            //openFileDialog1.ShowDialog();
            //string path = openFileDialog1.FileName;
            //pictureBox1.Image = Image.FromFile(path);
            //textPath.Text = path;
        }
    }
}


