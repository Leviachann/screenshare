using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ServerApp
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private BinaryReader reader;
        private int screenshotCounter = 0;
        private readonly string screenshotPath = "Screenshots/";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                StartListening();
            });
        }

        private void StartListening()
        {
            listener = new TcpListener(IPAddress.Any, 12345);
            listener.Start();

            Dispatcher.Invoke(() =>
            {
                ListenButton.IsEnabled = false;
            });

            while (true)
            {
                client = listener.AcceptTcpClient();
                stream = client.GetStream();
                reader = new BinaryReader(stream);

                ThreadPool.QueueUserWorkItem(o =>
                {
                    ReceiveAndDisplayScreenshot();
                });
            }
        }

        private void ReceiveAndDisplayScreenshot()
        {
            try
            {
                int imageSize = reader.ReadInt32();
                byte[] imageBytes = reader.ReadBytes(imageSize);

                using (MemoryStream ms = new MemoryStream(imageBytes))
                using (Bitmap bmp = new Bitmap(ms))
                {
                    string screenshotFilename = $"{screenshotPath}Screenshot_{screenshotCounter++}.jpg";
                    bmp.Save(screenshotFilename);
                    Console.WriteLine($"Received and saved: {screenshotFilename}");

                    Dispatcher.Invoke(() =>
                    {
                        DisplayScreenshot(bmp);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving screenshot: {ex.Message}");
            }
        }

        private void DisplayScreenshot(Bitmap screenshot)
        {
            ScreenshotImage.Source = ConvertBitmapToBitmapImage(screenshot);
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
