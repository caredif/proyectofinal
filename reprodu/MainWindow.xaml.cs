using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using System.Net;
using System.IO;

namespace reprodu
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {

        bool isDragging;


        public MainWindow()
        {
            InitializeComponent();

            //controles del temporizador media
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            isDragging = false;
        }

        void timer_Tick(object sender, EventArgs e)  //tiempo que esta reproduciendo
        {
            if (mePlayer.Source != null)
            {
                if (mePlayer.NaturalDuration.HasTimeSpan)
                    lblStatus.Content = String.Format("{0} / {1}", mePlayer.Position.ToString(@"mm\:ss"), mePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));

                if (!isDragging) // Si NO hay operación de arrastre en el sliderTimeLine, su posición se actualiza cada segundo.
                {
                    SliderTimeLine.Value = mePlayer.Position.TotalSeconds;
                }
            }
            else
                lblStatus.Content = "No file selected...";
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
        }

        private void Btnsiguiente_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SliderTimeLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //no usado
        {
            // isDragging = false;
            //MEmusicPlayer.Position =
            //    TimeSpan.FromSeconds(SliderTimeLine.Value);// La posición del MediaElement se actualiza para que coincida con el progreso del sliderTimeLine.

            //int sliderValue = (int)SliderTimeLine.Value;
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            //MEmusicPlayer.Position = ts;
        }

        private void SliderTimeLine_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void SliderTimeLine_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mePlayer.Position =
                TimeSpan.FromSeconds(SliderTimeLine.Value);// La posición del MediaElement se actualiza para que coincida con el progreso del sliderTimeLine.
        }

        private void SliderTimeLine_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mePlayer.Position =
                TimeSpan.FromSeconds(SliderTimeLine.Value);
        }

        private void BtnAtras_Click(object sender, RoutedEventArgs e)
        {

        }



        private void BtnDescargar_Click(object sender, RoutedEventArgs e)
        {

            Uri _videoUri = await GetYoutubeUri("1uP7AMW9bXg");

            if (_videoUri != null)
            {
                using (WebClient wc = new WebClient())
                {
                    //descargamos el vídeo a la carpeta donde se ejecuta la app como "video.mp4"
                    wc.DownloadFile(_videoUri, System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "video.mp4"));
                }
                // Ponemos como fuente el vídeo recién descargado
                mePlayer.Source = new Uri(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "video.mp4"));
            }               

        
        }


    }
        



 }

