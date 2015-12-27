using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fotobudka.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {

        Helper helper;
        System.Timers.Timer theTimer;
        System.Timers.Timer thePhotoTimer;
        ObservableCollection<ImageItem> imagesList;
        VideoCaptureDevice LocalWebCam;
        private VideoCaptureDevice videoDevice;
        private FilterInfoCollection videoDevices;
        public FilterInfoCollection LoaclWebCamsCollection;

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }));
            }
            catch (Exception ex)
            {
            }
        }

        public Home()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            helper = new Helper();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            LocalWebCam.SignalToStop();
            LocalWebCam.WaitForStop();
        }

     

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
                     
            imagesList = new ObservableCollection<ImageItem>();
            UserList.ItemsSource = imagesList;
            // Hook up the Elapsed event for the timer.
            

            // theTimer.Enabled = true;
            if (videoDevices == null)
            {
                // enumerate video devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count != 0)
                {
                    // add all devices to combo
                    foreach (FilterInfo device in videoDevices)
                    {
                        Cameras.Items.Add(device.Name);
                    }
                }
                else
                {
                    Cameras.Items.Add("No DirectShow devices found");
                }

                Cameras.SelectedIndex = 0;

            }
            if (LocalWebCam == null)
            {
                LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                LocalWebCam = videoDevice;
                LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

                LocalWebCam.Start();
            }
        }

        private void Cameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (videoDevices.Count != 0)
            {
                videoDevice = new VideoCaptureDevice(videoDevices[Cameras.SelectedIndex].MonikerString);

            }
        }

        private void makePhoto_Click(object sender, RoutedEventArgs e)
        {
            theTimer = new System.Timers.Timer(Settings.Settings1.Default.iTakePhotoDelay * 1000);
            theTimer.Elapsed += TheTimer_Elapsed;
            loops = 0;
            theTimer.Start();
         //   CreateBitmap();
        }
        int loops;
        private void TheTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerElapsed();
            theTimer.Stop();
            if (loops <= Settings.Settings1.Default.iPhotoCount)
            {
                thePhotoTimer = new System.Timers.Timer(Settings.Settings1.Default.iPhotoInterval * 1000);
                thePhotoTimer.Elapsed += ThePhotoTimer_Elapsed;
                thePhotoTimer.Start();
               
            }
            else
            {
                CreateBitmap();
            }

        
        }

        private void ThePhotoTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerElapsed();
            thePhotoTimer.Stop();
            if (loops < Settings.Settings1.Default.iPhotoCount)
                thePhotoTimer.Start();
            else
            {
                CreateBitmap();
            }
        }

        private void timerElapsed()
        {
            loops++;
           
            LocalWebCam.SimulateTrigger();
            ImageItem item = new ImageItem();
            BitmapImage im = new BitmapImage();
            this.Dispatcher.Invoke((Action)(() =>
            {
                im = (BitmapImage)frameHolder.Source;
                item.Image = im;

                imagesList.Add(item);

            }));
        }

        void CreateBitmap()
        {
            int imagesSetWidth = 0;
            int imagesSetHeight = 0;

            foreach ( ImageItem item in imagesList)
            {
                if(imagesSetWidth< (int)item.Image.Width + 100)
                    imagesSetWidth = (int)item.Image.Width + 50;
                imagesSetHeight = imagesSetHeight + (int)item.Image.Height +50;
            }


          //  imagesSetWidth = helper.CentimeterToPixel(10, true);
         //   imagesSetHeight = helper.CentimeterToPixel(15, false);

            System.Drawing.Bitmap flag = new System.Drawing.Bitmap(imagesSetWidth, imagesSetHeight);
            int height;

            int offset=0;
            using (Graphics grfx = Graphics.FromImage(flag))
            {
                grfx.Clear(System.Drawing.Color.White); 
                for (int i = 0; i < imagesList.Count; i++)
                {
                    height = (int)imagesList[0].Image.Height;
                   
                 Bitmap bmp = new Bitmap((int)imagesList[i].Image.Width, height);
                    bmp = (Bitmap)BitmapImage2Bitmap(imagesList[i].Image);

                    grfx.DrawImage(bmp, 25, offset+25);
                    offset = offset + height+25;
                }
            }

            flag.Save("temp.jpeg",ImageFormat.Jpeg);

            
        }


        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }

    public class ImageItem
    {
        public BitmapImage Image { get; set; }
       
    }

}
