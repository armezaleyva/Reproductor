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

using Microsoft.Win32;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using System.Windows.Threading;

namespace Reproductor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        DispatcherTimer timer;

        // Lector de archivos
        AudioFileReader reader;
        // Comunicación con la tarjeta de audio
        // Exclusivo para salidas
        WaveOut output;

        bool dragging = false;
        VolumeSampleProvider volume;

        public MainWindow() {
            InitializeComponent();
            ListarDispositivosSalida();
            btnReproducir.IsEnabled = false;
            btnPausa.IsEnabled = false;
            btnDetener.IsEnabled = false;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);

            if (!dragging) {
                sldTiempo.Value = reader.CurrentTime.TotalMilliseconds;
            }
        }

        void ListarDispositivosSalida() {
            cbDispositivoSalida.Items.Clear();
            for (int i = 0; i < WaveOut.DeviceCount; i++) {
                WaveOutCapabilities capacidades = WaveOut.GetCapabilities(i);
                cbDispositivoSalida.Items.Add(capacidades.ProductName);
            }
            cbDispositivoSalida.SelectedIndex = 0;
        }

        private void BtnExaminar_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                txtRutaArchivo.Text = openFileDialog.FileName;
                btnReproducir.IsEnabled = true;
            }
        }

        private void BtnReproducir_Click(object sender, RoutedEventArgs e) {
            if (output != null && output.PlaybackState == PlaybackState.Paused) {
                output.Resume();
                btnReproducir.IsEnabled = false;
                btnPausa.IsEnabled = true;
            }
            else {
                if (txtRutaArchivo.Text != null && txtRutaArchivo.Text != "") {
                    reader = new AudioFileReader(txtRutaArchivo.Text);
                    volume = new VolumeSampleProvider(reader);
                    volume.Volume = (float)sldVolumen.Value;

                    output = new WaveOut();
                    output.DeviceNumber = cbDispositivoSalida.SelectedIndex;
                    output.PlaybackStopped += Output_PlaybackStopped;
                    output.Init(volume);
                    output.Play();

                    // Cambiar el volumen del output
                    //output.Volume = (float)sldVolumen.Value;

                    btnReproducir.IsEnabled = false;
                    btnPausa.IsEnabled = true;
                    btnDetener.IsEnabled = true;

                    lblTiempoTotal.Text = reader.TotalTime.ToString().Substring(0, 8);
                    lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);
                    sldTiempo.Maximum = reader.TotalTime.TotalMilliseconds;

                    timer.Start();
                }
            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e) {
            timer.Stop();
            reader.Dispose();
            output.Dispose();
        }

        private void BtnDetener_Click(object sender, RoutedEventArgs e) {
            if (output != null) {
                output.Stop();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = false;
            }
        }

        private void BtnPausa_Click(object sender, RoutedEventArgs e) {
            if (output != null) {
                output.Pause();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
            }
        }

        private void SldTiempo_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            dragging = true;
        }

        private void SldTiempo_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            dragging = false;
            if (reader != null && output != null
                && output.PlaybackState != PlaybackState.Stopped) {
                reader.CurrentTime = TimeSpan.FromMilliseconds(sldTiempo.Value);
            }
        }

        private void SldVolumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (reader != null && output != null
                && output.PlaybackState != PlaybackState.Stopped) {
                //output.Volume = (float)sldVolumen.Value;
                volume.Volume = (float)sldVolumen.Value;
            }
        }
    }
}
