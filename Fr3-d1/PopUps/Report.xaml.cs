using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using Config.Net;
using FluentFTP;

namespace Fr3_d1.PopUps;

public partial class Report : Window
{
    private StatementLoc statementLoc = new StatementLoc();
    public Report(StatementLoc stin)
    {
        InitializeComponent();
        statementLoc = stin;
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

    private void Rep_OnClick(object sender, RoutedEventArgs e)
    {
        var stg = DateTime.UtcNow.ToShortTimeString().Replace(':', 'c').Replace(' ', 'c');
        System.IO.File.WriteAllText($"{statementLoc.Title}{stg}.report", reason.Text);
        if (!File.Exists("credentials"))
        {
            System.Net.WebClient Client = new System.Net.WebClient();
            byte[] result = Client.UploadFile(ConstantVars.UploadLink, "POST",
                $"{statementLoc.Title}{stg}.report");
            string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length); 
            MessageBox.Show(s);
        }
        else
        {
            var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
            using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
            {
                con.Connect();

                    if (!con.DirectoryExists($"moderated/{cr.Login}"))
                    {
                        con.CreateDirectory($"moderated/{cr.Login}");
                    }
                    con.UploadFile($"{statementLoc.Title}{stg}.report", $"moderated/{cr.Login}/{stg}.report");
                    con.Disconnect();
            }
        }
        File.Delete($"{statementLoc.Title}{stg}.report");
        this.Close();
    }
}