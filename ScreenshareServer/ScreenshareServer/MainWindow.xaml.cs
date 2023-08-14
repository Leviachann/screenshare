using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientApp
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private BinaryWriter writer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 12345);
                stream = client.GetStream();
                writer = new BinaryWriter(stream);

                // Enable UI for sending screenshots
                Dispatcher.Invoke(() =>
                {
                    LiveButton.IsEnabled = true;
                    ConnectButton.IsEnabled = false;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to the server: " + ex.Message);
            }
        }

        private void LiveButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    TakeAndSendScreenshot();
                    Thread.Sleep(200); // Adjust the interval as needed
                }
            });
        }

        private void TakeAndSendScreenshot()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            using (Bitmap bmp = new Bitmap((int)screenWidth, (int)screenHeight))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                byte[] imageBytes = ms.ToArray();
                writer.Write(imageBytes.Length);
                writer.Write(imageBytes);
            }
        }
    }
}
