using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Config.Net;
using FluentFTP;

namespace Fr3_d1.PopUps;

public partial class Edit : Window
{   
    public ArchiveConfig archiveConfig { get; set; }
    private StatementLoc statementInfo { get; set; } = new StatementLoc();
    public string type0 { get; set; }
    public Edit( StatementLoc statementinfo, string type)
    {
        InitializeComponent();
        statementInfo = statementinfo;
        type0 = type;
        if (!File.Exists($"{ConstantVars.StatementsPath}/config.ini"))
        {
            MessageBox.Show("Please, sync with The Web.");
        }
        else
        {
            archiveConfig = new ConfigurationBuilder<ArchiveConfig>().UseIniFile($"{ConstantVars.StatementsPath}/config.ini").Build();
            ini();
        }
    }
    
    public void ini()
    {
        try
        {
            string path = "";
            switch (type0)
            {
                case "n":
                    path = ConstantVars.StatementsPath;
                    break;
                case "u":
                    var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                    using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                    {
                        con.Connect();
                        if (con.DirectoryExists("unmoderated"))
                        {
                            path = "unmoderated";
                        }
                        else if (con.DirectoryExists("main"))
                        {
                            path = "unmoderated\\moderation\\unmoderated";
                        }
                    }
                    break;
                case "m":
                    var cr2 = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                    using (var con = new FtpClient("31.31.196.95", cr2.Login, cr2.Password))
                    {
                        con.Connect();
                        if (con.DirectoryExists("unmoderated"))
                        {
                            path = "moderated";
                        }
                        else if (con.DirectoryExists("main"))
                        {
                            path = "moderated\\moderation\\moderated";
                        }
                    }
                    break;
            }
            
            foreach (var v in archiveConfig.tags.Split("$"))
            {
                System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox();
                checkBox.Content = v;
                tags.Children.Add(checkBox);
            }
            ZipFile.ExtractToDirectory($"{path}/{statementInfo.Title}.statement", $"{statementInfo.Title}.edit", true);
            Statement draft = new ConfigurationBuilder<Statement>().UseIniFile($"{statementInfo.Title}.edit/{statementInfo.Title}.statementinfo").Build();
            sttitle.Text = draft.Title;
            streg.Text = draft.Regarding;
            stof.Text = draft.StatementOf;
            if (draft.StatementGiven != null)
            {
                stgivendate.SelectedDate = DateTime.ParseExact(draft.StatementGiven, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            if (draft.StatementDigitized != null)
            {
                stdigdate.SelectedDate =  DateTime.ParseExact(draft.StatementDigitized, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            stdigby.Text = draft.DigitizedBy;
            foreach (var tgs in tags.Children.OfType<System.Windows.Controls.CheckBox>())
            {
                if (draft.Tags.Contains(tgs.Content.ToString()))
                {
                    tgs.IsChecked = true;
                }
            }
            TextRange range;
            FileStream fStream;
            range = new TextRange(theStatement.Document.ContentStart, theStatement.Document.ContentEnd);
            fStream = new FileStream($"{statementInfo.Title}.edit/{statementInfo.Title}.statementcontent", FileMode.OpenOrCreate);
            range.Load(fStream, System.Windows.DataFormats.XamlPackage);
            fStream.Close();
            Directory.Delete($"{statementInfo.Title}.edit", true);
            stdigdate.SelectedDate = DateTime.Now;
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
    
    private void collapse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
        this.WindowState = System.Windows.WindowState.Minimized;
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    private void fullsc_Click(object sender, RoutedEventArgs e)
    {
        try
        {
        if (this.WindowState == WindowState.Normal)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }
        else
        {
            WindowState = WindowState.Normal;
        }
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
    private void Send_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var stg = DateTime.UtcNow.ToShortTimeString().Replace(':', 'c').Replace(' ', 'c');
            if (!File.Exists($"{ConstantVars.StatementsPath}/config.ini"))
            {
                MessageBox.Show("Please, sync with The Web.");
                return;
            }
        var nm = sttitle.Text;
        Directory.CreateDirectory($"dir{nm}.statement");
        Statement draft = new ConfigurationBuilder<Statement>().UseIniFile($"dir{nm}.statement/{nm}.statementinfo").Build();
        draft.Title = sttitle.Text;
        draft.Regarding = streg.Text;
        draft.StatementOf = stof.Text;
        if (stgivendate.SelectedDate != null)
        {
            draft.StatementGiven =  stgivendate.SelectedDate.Value.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
        if (stdigdate.SelectedDate != null)
        {
            draft.StatementDigitized  =  stdigdate.SelectedDate.Value.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
        draft.DigitizedBy = stdigby.Text;
        List<string> tagss = new List<string>();
        foreach (var tgs in tags.Children.OfType<System.Windows.Controls.CheckBox>())
        {
            if (tgs.IsChecked == true)
            {
                tagss.Add(tgs.Content.ToString());
            }
        }
        draft.Tags = string.Join('$', tagss);
        TextRange range;
        FileStream fStream;
        range = new TextRange(theStatement.Document.ContentStart, theStatement.Document.ContentEnd);
        fStream = new FileStream($"dir{nm}.statement/{nm}.statementcontent", FileMode.Create);
        range.Save(fStream, System.Windows.DataFormats.XamlPackage);
        fStream.Close();
        fStream.Dispose();
        TextRange range2;
        FileStream fStream2;
        range2 = new TextRange(Reason.Document.ContentStart, Reason.Document.ContentEnd);
        fStream2 = new FileStream($"dir{nm}.statement/{nm}.statementeditreason", FileMode.Create);
        range2.Save(fStream2, System.Windows.DataFormats.XamlPackage);
        fStream2.Close();
        fStream2.Dispose();
        if (File.Exists($"{nm}.statement"))
        {
            File.Delete($"{nm}.statement");
        }
        draft = null;
        ZipFile.CreateFromDirectory($"dir{nm}.statement", $"{nm}{stg}.edit");
        Directory.Delete($"dir{nm}.statement", true);
        try
        {
            if (!File.Exists("credentials"))
            {
                System.Net.WebClient Client = new System.Net.WebClient();
                byte[] result = Client.UploadFile(ConstantVars.UploadLink, "POST",
                    $"{nm}{stg}.edit");
                string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length); 
                MessageBox.Show(s);
            }
            else
            {
                var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                {
                    con.Connect();
                    if (con.DirectoryExists("main"))
                    {
                        con.DownloadFile("main.archive", "main/main.archive");
                        ZipFile.ExtractToDirectory("main.archive", "main.archive.edit", true);
                        File.Delete("main.archive");
                        File.Move($"{nm}{stg}.edit",$"main.archive.edit/{nm}.statement", true );
                        ZipFile.CreateFromDirectory("main.archive.edit", "main.archive");
                        con.UploadFile("main.archive", "main/main.archive", FtpRemoteExists.Overwrite);
                        Directory.Delete("main.archive.edit", true);
                    }
                    else
                    {
                        if (!con.DirectoryExists($"moderated/{cr.Login}/{nm}{stg}.edit"))
                        {
                            con.CreateDirectory($"moderated/{cr.Login}");
                        }
                        con.UploadFile($"{nm}{stg}.edit", $"moderated/{cr.Login}/{nm}{stg}.edit", FtpRemoteExists.Overwrite);
                    }
                    con.Disconnect();
                }
            }
            
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        File.Delete($"{nm}{stg}.edit");
        this.Close();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
}