using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.Wave;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Fr3_d1.PopUps;

public partial class SyncProcess : Window
{
    public static EventHandler upd0 { get; set; }
    private DispatcherTimer soundTimer { get; set; } = new DispatcherTimer();
    private AudioFileReader audioFileReader { get; set; } = new AudioFileReader("Sounds/connection");
    private WaveOutEvent waveOutEvent { get; set; } = new WaveOutEvent();
    private WebClient webClient { get; set; } = new WebClient();
    public SyncProcess()
    {
        InitializeComponent();
        soundTimer.Tick += SoundTimerOnTick;
        waveOutEvent.PlaybackStopped += WaveOutEventOnPlaybackStopped;
    }

    private void WaveOutEventOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        try
        {
            SyncButton.IsEnabled = true;
            progressBar.IsIndeterminate = true;
            ConnectButton.Content = "Disconnect";
            title.Text = "The Web - Connected";
            soundTimer.Stop();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
        
    }

    private void SoundTimerOnTick(object? sender, EventArgs e)
    {
        try
        {
        var dec = audioFileReader.CurrentTime.TotalMilliseconds / audioFileReader.TotalTime.TotalMilliseconds;
        progressBar.Value = (double)dec*100;
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    private void close_Click(object sender, RoutedEventArgs e)
    {
        try
        {
        this.Close();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
        this.DragMove();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
        if ((sender as System.Windows.Controls.Button).Content.ToString() == "Connect")
        {
            waveOutEvent.Init(audioFileReader);
            waveOutEvent.Play();
            soundTimer.Start();
            close.IsEnabled = false;
            ConnectButton.IsEnabled = false;
            title.Text = "The Web - Connecting";
        }
        else
        {
            close.IsEnabled = true;
            SyncButton.IsEnabled = false;
            title.Text = "The Web - Stand By";
            progressBar.IsIndeterminate = false;
            progressBar.Value = 0;
        }
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    private async void SyncButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
        progressBar.IsIndeterminate = false;
        progressBar.Value = 0;
        SyncButton.IsEnabled = false;
        var updatelink = "https://www.triangleonthewall.org/statements/main/main.archive";
        File.Delete("main.archive");
        title.Text = "The Web - Reading";
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(new System.Uri(updatelink), HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            title.Text = ("Error: " + response.StatusCode);
            return;
        }
        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;
        var totalBytesRead = 0L;
        var readChunkSize = 8192;
        using (var contentStream = await response.Content.ReadAsStreamAsync())
        using (var fileStream = new FileStream("main.archive", FileMode.Create, FileAccess.Write, FileShare.None, readChunkSize, true))
        {
            var buffer = new byte[readChunkSize];
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
                if (canReportProgress)
                {
                    var progressPercentage = Math.Round((double)totalBytesRead / totalBytes * 100, 0);
                    progressBar.Value = progressPercentage;
                }
            }
        }
        client_DownloadFileCompleted();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
    public void wait()
    {
        try
        {
        Thread.Sleep(1000);
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
    public DispatcherTimer prg = new DispatcherTimer();
    void client_DownloadFileCompleted()
    {
        try
        {
        prg.Tick += PrgOnTick;
        progressBar.Value = 0;
        prg.Start();
        if (Directory.Exists(ConstantVars.StatementsPath))
        {
            Directory.Delete(ConstantVars.StatementsPath, true);
        }
        Directory.CreateDirectory(ConstantVars.StatementsPath);
        ZipFile.ExtractToDirectory("main.archive", ConstantVars.StatementsPath, true );
        DirectoryInfo stts = new DirectoryInfo(ConstantVars.StatementsPath);
        File.Delete("main.archive");
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
    public Random rnd = new Random();
    private async void PrgOnTick(object? sender, EventArgs e)
    {
        try
        {
        title.Text = "The Web - Getting data";
        if (progressBar.Value < 100)
        {
            progressBar.Value += 1;
            Task t = new Task(wait);
            Thread.Sleep(rnd.Next(0,500));
        }
        else
        {
            MessageBox.Show("Synchronization is finished, please disconnect from The Web");
            progressBar.IsIndeterminate = true;
            ConnectButton.IsEnabled = true;
            upd0(sender, e);
            prg.Stop();
        }
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
}