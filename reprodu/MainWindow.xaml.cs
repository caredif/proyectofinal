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
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using Microsoft.Win32;

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

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
        }


        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MediaPlayerAudioControlSample()
        {
            InitializeComponent();

           

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
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


        private static string NormalizeVideoId(string input)
        {
            string videoId = string.Empty;
            return YoutubeClient.TryParseVideoId(input, out videoId)
                ? videoId
                : input;
        }


        private async void BtnDescargar_Click(object sender, RoutedEventArgs e)
        {

            /*  Uri _videoUri = await GetYoutubeUri("1uP7AMW9bXg");

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
              */

            

            //nuevo cliente de Youtube
            var client = new YoutubeClient();
            //lee la dirección de youtube que le escribimos en el textbox
            var videoId = NormalizeVideoId(txtYoutubeURL.Text);
            var video = await client.GetVideoAsync(videoId);
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            // Busca la mejor resolución en la que está disponible el video
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            // Compone el nombre que tendrá el video en base a su título y extensión
            var fileExtension = streamInfo.Container.GetFileExtension();
            var fileName = $"{video.Title}.{fileExtension}";

            //TODO: Reemplazar los caractéres ilegales del nombre
            //fileName = RemoveIllegalFileNameChars(fileName);

            //Activa el timer para que el proceso funcione de forma asincrona
            //tmrVideo.Enabled = true;
            

            // mensajes indicando que el video se está descargando
            //txtMensaje.Text = "Descargando el video ... ";

            //TODO: se pude usar una barra de progreso para ver el avance
            //using (var progress = new ProgressBar())

            //Empieza la descarga
            await client.DownloadMediaStreamAsync(streamInfo, fileName);

            //Ya descargado se inicia la conversión a MP3
            var Convert = new NReco.VideoConverter.FFMpegConverter();
            //Especificar la carpeta donde se van a guardar los archivos, recordar la \ del final
            
            String SaveMP3File = @"C:\DescargasPF" + fileName.Replace(".mp4", ".mp3");
            //Guarda el archivo convertido en la ubicación indicada
            Convert.ConvertMedia(fileName, SaveMP3File, "mp3");

            //Si el checkbox de solo audio está chequeado, borrar el mp4 despues de la conversión
            /*if (ckbAudio.Checked)
                File.Delete(fileName);*/


            //Indicar que se terminó la conversion
            txtMensaje.Text = "Archivo Convertido en MP3";
            //tmrVideo.Enabled = false;
            //txtMensaje.BackColor = Color.White;

            //TODO: Cargar el MP3 al reproductor o a la lista de reproducción
            //CargarMP3s();
            //Se puede incluir un checkbox para indicar que de una vez se reproduzca el MP3
            //if (ckbAutoPlay.Checked) 
            //  ReproducirMP3(SaveMP3File);

        }


    }
        



 }

