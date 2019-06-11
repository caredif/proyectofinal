using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace reprodu
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        DispatcherTimer timer;
        bool isDragging;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private object nombre;

        public MainWindow()
        {
            InitializeComponent();

            //controles del temporizador media
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            //timer.Tick += timer_Tick;
            timer.Start();

            isDragging = false;

            /*
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mediaPlayer.Open(new Uri(openFileDialog.FileName));
            */
        }


        void timer_Tick(object sender, EventArgs e)  //tiempo que esta reproduciendo
        {

            /*
            if (mediaPlayer.Source != null)
                lblStatus.Content = String.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            else
                lblStatus.Content = "No file selected...";*/

            if (mediaPlayer.Source != null)
            {
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    lblStatus.Content = string.Format("{0} / {1}", mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                    TimeSpan ts = mediaPlayer.NaturalDuration.TimeSpan;
                    SliderTimeLine.Maximum = ts.TotalSeconds;
                    SliderTimeLine.SmallChange = 1;
                }
                    
            }
            else
                lblStatus.Content = "No file selected...";

            if (!isDragging) // Si NO hay operación de arrastre en el sliderTimeLine, su posición se actualiza cada segundo.
            {
                SliderTimeLine.Value = mediaPlayer.Position.TotalSeconds;
            }

        }



        private void MEmusicPlayer_MediaEnded(object sender, RoutedEventArgs e) // Se dispara cuando se termina de reproducir una canción pero no cuando se hace click en Stop
        {
            SliderTimeLine.Value = 0;
            //MoveToNextTrack();
        }


        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            SliderTimeLine.Value = 0;
            mediaPlayer.Play();
        }




        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void Btnsiguiente_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SliderTimeLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //no usado
        {
            // isDragging = false;
            //mediaPlayer.Position =
            //    TimeSpan.FromSeconds(SliderTimeLine.Value);// La posición del MediaElement se actualiza para que coincida con el progreso del sliderTimeLine.

            //int sliderValue = (int)SliderTimeLine.Value;
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            //mediaPlayer.Position = ts;
        }

        private void SliderTimeLine_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void SliderTimeLine_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(SliderTimeLine.Value);// La posición del MediaElement se actualiza para que coincida con el progreso del sliderTimeLine.
        }

        private void SliderTimeLine_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(SliderTimeLine.Value);
        }

        private void BtnAtras_Click(object sender, RoutedEventArgs e)
        {

        }

        
        private static string NormalizeVideoId(string input)
        {
            string videoId;     // = string.Empty;
            return YoutubeClient.TryParseVideoId(input, out videoId)
                ? videoId
                : input;
        }


        private async void BtnDescargar_Click(object sender, RoutedEventArgs e)
        {

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
            fileName = CleanInput(fileName);


            //Activa el timer para que el proceso funcione de forma asincrona
            //tmrVideo.Enabled = true;


            // mensajes indicando que el video se está descargando
            //txtMensaje.Text = "Descargando el video ... ";
            MessageBox.Show("Descargando... porfavor espere...");
            pbStatus.Visibility = Visibility.Visible;
            txtdescargando.Visibility = Visibility.Visible;

            //TODO: se pude usar una barra de progreso para ver el avance
            //using (ProgressBar progress = new ProgressBar())

            //Empieza la descarga
            await client.DownloadMediaStreamAsync(streamInfo, fileName);
       

            //Ya descargado se inicia la conversión a MP3
            var Convert = new NReco.VideoConverter.FFMpegConverter();
            //Especificar la carpeta donde se van a guardar los archivos, recordar la \ del final
            
            String SaveMP3File = @"C:\DescargasPF\" + fileName.Replace(".mp4", ".mp3");
            //Guarda el archivo convertido en la ubicación indicada
            Convert.ConvertMedia(fileName, SaveMP3File, "mp3");


            //Si el checkbox de solo audio está chequeado, borrar el mp4 despues de la conversión
            //if (ckbAudio.Checked)
             //   File.Delete(fileName);


            //Indicar que se terminó la conversion
            MessageBox.Show("Archivo descargado y convertido");
            pbStatus.Visibility = Visibility.Hidden;
            txtdescargando.Visibility = Visibility.Hidden;

            //TODO: Cargar el MP3 al reproductor o a la lista de reproducción
            mediaPlayer.Open(new Uri(SaveMP3File));

            Cancioneslist CancionesJson = new Cancioneslist();
            CancionesJson.Nombre = fileName;
            CancionesJson.Ubicacion = SaveMP3File;

            string salida = JsonConvert.SerializeObject(CancionesJson);
            FileStream stream = new FileStream("Biblioteca.Json", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(salida);
            writer.Close();
            MessageBox.Show("Ingresado Exitosamente!!");

        }

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", " ",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            List<Cancioneslist> listacanciones = new List<Cancioneslist>();

            FileStream stream = new FileStream("Biblioteca.json", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            while (reader.Peek() > -1)
            {
                string lectura = reader.ReadLine();
                Cancioneslist libroLeido = JsonConvert.DeserializeObject<Cancioneslist>(lectura);
                listacanciones.Add(libroLeido);
            }
            reader.Close();

            listBoxBiblio.ItemsSource = listacanciones;
            listBoxBiblio.DisplayMemberPath = "Nombre";
            
        }

        private void Crear_Click(object sender, RoutedEventArgs e)
        {

        }
    }
        



 }

