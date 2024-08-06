using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Config.Net;
using FluentFTP;
using Fr3_d1.PopUps;




namespace Fr3_d1
{


    public static class ConstantVars
    {
        public static string UploadLink { get; } = "https://triangleonthewall.org/statements/upload.php";
        public static string StatementsPath { get; } = "Statements";
    }
    public interface MainConfig
    {
        public int FontSize { get; set; }
        public string TextBackgroundColor { get; set; }
        public string PrimaryColor { get; set; }
        public string Favs { get; set; }
        public int ver { get; set; }
    }
    public interface ArchiveConfig
    {
        public string tags { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //statements
        public List<StatementLoc> StatementsInf { get; set; }
        public List<StatementLoc> StatementListRN { get; set; }
        public List<FlowDocument> StatementsCont { get; set; }
        
        //unmoderated
        public List<StatementLoc> UStatementsInf { get; set; }
        public List<StatementLoc> UStatementListRN { get; set; }
        public List<FlowDocument> UStatementsCont { get; set; }
        
        //moderated
        public List<StatementLoc> MStatementsInf { get; set; }
        public List<StatementLoc> MStatementListRN { get; set; }
        public List<FlowDocument> MStatementsCont { get; set; }
        
        //vars
        public ArchiveConfig archiveConfig { get; set; }
        public MainConfig mainConfig { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string modpath { get; set; }
        public string umodpath { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            SyncProcess.upd0 += Upd0;
            ini();
        }
        
        
        //controls
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
                Application.Current.Shutdown();
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
        
        
        //init
        public void ini()
        {
            try
            {
                IsAdmin = false;
            StatementsListBox.ItemsSource = null;
            commontags.Items.Clear();
            mainConfig = new ConfigurationBuilder<MainConfig>().UseIniFile("MainConfig.ini").Build();
            
            if (Directory.Exists(ConstantVars.StatementsPath))
            {
                if (File.Exists($"{ConstantVars.StatementsPath}/config.ini"))
                {
                    archiveConfig = new ConfigurationBuilder<ArchiveConfig>().UseIniFile($"{ConstantVars.StatementsPath}/config.ini")
                        .Build();
                    commontags.Items.Clear();
                    foreach (var tag in archiveConfig.tags.Split("$"))
                    {
                        TreeViewItem treeViewItem = new TreeViewItem();
                        treeViewItem.Header = tag;
                        treeViewItem.Name = tag.Replace(" ", "_");
                        treeViewItem.Selected += TreeViewItemOnSelected;
                        commontags.Items.Add(treeViewItem);
                    }
                }
                StatementsCont = new List<FlowDocument>();
                StatementsInf = new List<StatementLoc>();
                StatementListRN = new List<StatementLoc>();
                DirectoryInfo statdir = new DirectoryInfo(ConstantVars.StatementsPath);
                foreach (var dinf in statdir.GetDirectories())
                {
                    if (dinf.Name.Contains("temp"))
                    {
                        Directory.Delete(dinf.FullName, true);
                    }
                }
                var stats = statdir.GetFiles();
                StatementsListBox.Items.Clear();
                foreach (var statement in stats)
                {
                    if (statement.Name.Contains(".statement"))
                    {
                        ZipFile.ExtractToDirectory(statement.FullName, $"{statement.Directory.FullName}/temp.{statement.Name}", true);
                        FlowDocument flowDocument = new FlowDocument();
                            Block head = new Paragraph();
                            FlowDocument main = new FlowDocument();
                            DirectoryInfo tfd = new DirectoryInfo($"{statement.Directory.FullName}/temp.{statement.Name}");
                            foreach (var whatever in tfd.GetFiles())
                            {
                                if (whatever.Name.Contains(".statementinfo"))
                                {
                                    Statement reminfo = new ConfigurationBuilder<Statement>().UseIniFile(whatever.FullName)
                                        .Build();
                                    Statement info = reminfo;
                                    info.LocId = StatementsInf.Count;
                                    StatementsInf.Add(new StatementLoc()
                                    {
                                        StatementGiven = info.StatementGiven, StatementOf = info.StatementOf, StatementDigitized = info.StatementDigitized,
                                        TheStatement = info.TheStatement, Regarding = info.Regarding, Tags = info.Tags, Title = info.Title, DigitizedBy = info.DigitizedBy, LocId = info.LocId
                                    });
                                    head = new Paragraph(new Run(
                                        $"{info.Title}\nStatement of {info.StatementOf}, regarding {info.Regarding}." +
                                        $" Statement given on {info.StatementGiven}, digitized on {info.StatementDigitized} by " +
                                        $"{info.DigitizedBy}.\n Statement begins"))
                                    {
                                        TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold, FontSize = 20
                                    };
                                    StatementListRN.Clear();
                                    StatementListRN.AddRange(StatementsInf);
                                }
                                else if(whatever.Name.Contains(".statementcontent"))
                                {
                                    TextRange range;
                                    FileStream fStream;
                                    range = new TextRange(main.ContentStart, main.ContentEnd);
                                    fStream = new FileStream(whatever.FullName, FileMode.OpenOrCreate);
                                    range.Load(fStream, System.Windows.DataFormats.XamlPackage);
                                    fStream.Close();
                                }
                            }
                            flowDocument.Blocks.Add(head);
                            flowDocument.Blocks.AddRange(main.Blocks.ToList());
                            StatementsCont.Add(flowDocument);
                            Directory.Delete($"{statement.Directory.FullName}/temp.{statement.Name}", true);
                    }
                }
                StatementsListBox.ItemsSource = StatementListRN;
            }
            if (File.Exists("credentials"))
            {
                try
                {
                    var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                    using( var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                    {
                        con.Connect();
                        con.Disconnect();
                    };
                    AdminIni();
                }
                catch (Exception exception)
                {
                    Console.Write("f");
                }
            }
            else
            {
                ttle.Text = "Fr3-d1 v.2.0.2 - Archive Worker Edition";
                IsAdmin = false;
                Moderated.Visibility = Visibility.Hidden;
                Unmoderated.Visibility = Visibility.Hidden;
            }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        
        
        //admin
        public void AdminIni()
        {
            var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
            using( var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
            {
                con.Connect();
                if (con.DirectoryExists("unmoderated"))
                {
                    ttle.Text = "Fr3-d1 v.2.0.2 - Archival Assistant Edition";
                    Unmoderated.Visibility = Visibility.Visible;
                    Directory.CreateDirectory("unmoderated");
                    con.DownloadDirectory("unmoderated", "unmoderated", FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite, FtpVerify.Retry);
                    umodpath = "unmoderated/unmoderated/";
                    IsAdmin = true;
                }
                else if (con.DirectoryExists("main"))
                {
                    ttle.Text = "Fr3-d1 v.2.0.2 - The Head Archivist Edition";
                    Moderated.Visibility = Visibility.Visible;
                    Unmoderated.Visibility = Visibility.Visible;
                    con.DownloadDirectory("unmoderated", "moderation/unmoderated", FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite, FtpVerify.Retry);
                    con.DownloadDirectory("moderated", "moderation/moderated", FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite, FtpVerify.Retry);
                    umodpath = "unmoderated/moderation/unmoderated";
                    modpath = "moderated/moderation/moderated";
                    IsAdmin = true;
                }
                login.Header = "Log out";
                con.Disconnect();
                GetUnmod();
                GetMod();
            };
        }
        public void GetUnmod()
        {
            UStatementsCont = new List<FlowDocument>();
                UStatementsInf = new List<StatementLoc>();
                UStatementListRN = new List<StatementLoc>();
                DirectoryInfo statdir = new DirectoryInfo(umodpath);
                foreach (var dinf in statdir.GetDirectories())
                {
                    if (dinf.Name.Contains("temp"))
                    {
                        Directory.Delete(dinf.FullName, true);
                    }
                }
                var stats = statdir.GetFiles();
                foreach (var statement in stats)
                {
                    if (statement.Name.Contains(".statement"))
                    {
                        ZipFile.ExtractToDirectory(statement.FullName, $"{statement.Directory.FullName}/temp.{statement.Name}", true);
                        FlowDocument flowDocument = new FlowDocument();
                            Block head = new Paragraph();
                            FlowDocument main = new FlowDocument();
                            DirectoryInfo tfd = new DirectoryInfo($"{statement.Directory.FullName}/temp.{statement.Name}");
                            foreach (var whatever in tfd.GetFiles())
                            {
                                if (whatever.Name.Contains(".statementinfo"))
                                {
                                    Statement reminfo = new ConfigurationBuilder<Statement>().UseIniFile(whatever.FullName)
                                        .Build();
                                    Statement info = reminfo;
                                    info.LocId = UStatementsInf.Count;
                                    UStatementsInf.Add(new StatementLoc()
                                    {
                                        StatementGiven = info.StatementGiven, StatementOf = info.StatementOf, StatementDigitized = info.StatementDigitized,
                                        TheStatement = info.TheStatement, Regarding = info.Regarding, Tags = info.Tags, Title = info.Title, DigitizedBy = info.DigitizedBy, LocId = info.LocId
                                    });
                                    head = new Paragraph(new Run(
                                        $"{info.Title}\nStatement of {info.StatementOf}, regarding {info.Regarding}." +
                                        $" Statement given on {info.StatementGiven}, digitized on {info.StatementDigitized} by " +
                                        $"{info.DigitizedBy}.\n Statement begins"))
                                    {
                                        TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold, FontSize = 20
                                    };
                                    UStatementListRN.Clear();
                                    UStatementListRN.AddRange(UStatementsInf);
                                }
                                else
                                {
                                    TextRange range;
                                    FileStream fStream;
                                    range = new TextRange(main.ContentStart, main.ContentEnd);
                                    fStream = new FileStream(whatever.FullName, FileMode.OpenOrCreate);
                                    range.Load(fStream, System.Windows.DataFormats.XamlPackage);
                                    fStream.Close();
                                }
                            }
                            flowDocument.Blocks.Add(head);
                            flowDocument.Blocks.AddRange(main.Blocks.ToList());
                            UStatementsCont.Add(flowDocument);
                            Directory.Delete($"{statement.Directory.FullName}/temp.{statement.Name}", true);
                    }
                }
        }
        public void GetMod()
        {
            MStatementsCont = new List<FlowDocument>();
                MStatementsInf = new List<StatementLoc>();
                MStatementListRN = new List<StatementLoc>();
                DirectoryInfo statdir = new DirectoryInfo(modpath);
                foreach (var dinf in statdir.GetDirectories())
                {
                    if (dinf.Name.Contains("temp"))
                    {
                        Directory.Delete(dinf.FullName, true);
                    }
                }
                var stats = statdir.GetFiles();
                StatementsListBox.Items.Clear();
                foreach (var statement in stats)
                {
                    if (statement.Name.Contains(".statement"))
                    {
                        ZipFile.ExtractToDirectory(statement.FullName, $"{statement.Directory.FullName}/temp.{statement.Name}", true);
                        FlowDocument flowDocument = new FlowDocument();
                            Block head = new Paragraph();
                            FlowDocument main = new FlowDocument();
                            DirectoryInfo tfd = new DirectoryInfo($"{statement.Directory.FullName}/temp.{statement.Name}");
                            foreach (var whatever in tfd.GetFiles())
                            {
                                if (whatever.Name.Contains(".statementinfo"))
                                {
                                    Statement reminfo = new ConfigurationBuilder<Statement>().UseIniFile(whatever.FullName)
                                        .Build();
                                    Statement info = reminfo;
                                    info.LocId = MStatementsInf.Count;
                                    MStatementsInf.Add(new StatementLoc()
                                    {
                                        StatementGiven = info.StatementGiven, StatementOf = info.StatementOf, StatementDigitized = info.StatementDigitized,
                                        TheStatement = info.TheStatement, Regarding = info.Regarding, Tags = info.Tags, Title = info.Title, DigitizedBy = info.DigitizedBy, LocId = info.LocId
                                    });
                                    head = new Paragraph(new Run(
                                        $"{info.Title}\nStatement of {info.StatementOf}, regarding {info.Regarding}." +
                                        $" Statement given on {info.StatementGiven}, digitized on {info.StatementDigitized} by " +
                                        $"{info.DigitizedBy}.\n Statement begins"))
                                    {
                                        TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold, FontSize = 20
                                    };
                                    MStatementListRN.Clear();
                                    MStatementListRN.AddRange(MStatementsInf);
                                }
                                else
                                {
                                    TextRange range;
                                    FileStream fStream;
                                    range = new TextRange(main.ContentStart, main.ContentEnd);
                                    fStream = new FileStream(whatever.FullName, FileMode.OpenOrCreate);
                                    range.Load(fStream, System.Windows.DataFormats.XamlPackage);
                                    fStream.Close();
                                }
                            }
                            flowDocument.Blocks.Add(head);
                            flowDocument.Blocks.AddRange(main.Blocks.ToList());
                            MStatementsCont.Add(flowDocument);
                            Directory.Delete($"{statement.Directory.FullName}/temp.{statement.Name}", true);
                    }
                }
        }
        
        
        //menu
        //file
        private void Sync_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SyncProcess syncProcess = new SyncProcess();
                syncProcess.Owner = this;
                syncProcess.ShowDialog();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void clrDat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.Delete(ConstantVars.StatementsPath, true);
                Directory.CreateDirectory(ConstantVars.StatementsPath);
                ini();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void Login_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Header.ToString() == "Log in")
            {
                LogIn logIn = new LogIn();
                logIn.ShowDialog();
                if (File.Exists("credentials"))
                {
                    try
                    {
                        var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                        using( var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                        {
                            con.Connect();
                            con.Disconnect();
                        };
                        AdminIni();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
            else
            {
                File.Delete("credentials");
                (sender as MenuItem).Header = "Log in";
                ini();
            }
        }

        
        //edit
        private void newStatement_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NewStatement newStatementWin = new NewStatement();
                newStatementWin.Show();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        //view
        private void FontSmaller_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void FontBigger_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        
        //statement
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StatementsListBox.SelectedItem == null)
                {
                    return;
                }
                if (mainConfig.Favs == null || !mainConfig.Favs.Contains($"{((StatementsListBox.SelectedItem as StatementLoc).LocId)}$"))
                {
                    mainConfig.Favs += $"{((StatementsListBox.SelectedItem as StatementLoc).LocId)}$";
                }
                else
                {
                    mainConfig.Favs.Replace($"{((StatementsListBox.SelectedItem as StatementLoc).LocId)}$", "");
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private async void Accept_OnClick(object sender, RoutedEventArgs e)
        {
            if (Unmoderated.IsSelected)
            {
                var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                {
                    con.Connect();
                    if (con.DirectoryExists("unmoderated"))
                    {
                        con.MoveFile(
                            $"unmoderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"denied/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                    }
                    else if (con.DirectoryExists("main"))
                    {
                        con.DownloadFile("main.archive", "main/main.archive");
                        ZipFile.ExtractToDirectory("main.archive", "main.archive.edit", true);
                        File.Delete("main.archive");
                        con.DownloadFile($"main.archive.edit/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"moderation/unmoderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                        con.DeleteFile($"main.archive.edit/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                        ZipFile.CreateFromDirectory("main.archive.edit", "main.archive");
                        con.UploadFile("main.archive", "main/main.archive", FtpRemoteExists.Overwrite);
                        Directory.Delete("main.archive.edit", true);
                    }
                }
            }
            else if (Moderated.IsSelected)
            {
                var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                {
                    con.Connect();
                    if (con.DirectoryExists("unmoderated"))
                    {
                        con.MoveFile(
                            $"unmoderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"denied/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                        return;
                    }
                    else if (con.DirectoryExists("main"))
                    {
                        con.DownloadFile("main.archive", "main/main.archive");
                        ZipFile.ExtractToDirectory("main.archive", "main.archive.edit", true);
                        File.Delete("main.archive");
                        con.DownloadFile($"main.archive.edit/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"moderation/moderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                        con.DeleteFile($"main.archive.edit/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                        ZipFile.CreateFromDirectory("main.archive.edit", "main.archive");
                        con.UploadFile("main.archive", "main/main.archive", FtpRemoteExists.Overwrite);
                        Directory.Delete("main.archive.edit", true);
                    }
                }
            }
            var updatelink = "https://www.triangleonthewall.org/statements/main/main.archive";
            File.Delete("main.archive");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new System.Uri(updatelink), HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error: " + response.StatusCode);
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
                }
            }
            if (Directory.Exists(ConstantVars.StatementsPath))
            {
                Directory.Delete(ConstantVars.StatementsPath, true);
            }
            Directory.CreateDirectory(ConstantVars.StatementsPath);
            ZipFile.ExtractToDirectory("main.archive", ConstantVars.StatementsPath, true );
            DirectoryInfo stts = new DirectoryInfo(ConstantVars.StatementsPath);
            File.Delete("main.archive");
            ini();
        }
        private void Deny_OnClick(object sender, RoutedEventArgs e)
        {
            if (Unmoderated.IsSelected)
            {
                var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                {
                    con.Connect();
                    if (con.DirectoryExists("unmoderated"))
                    {
                        con.MoveFile(
                            $"unmoderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"denied/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                    }
                    else if (con.DirectoryExists("main"))
                    {
                        con.MoveFile(
                            $"moderation/unmoderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"moderation/denied/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                    }
                }
            }
            else if (Moderated.IsSelected)
            {
                var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                {
                    con.Connect();
                    if (con.DirectoryExists("unmoderated"))
                    {
                       return;
                    }
                    else if (con.DirectoryExists("main"))
                    {
                        con.MoveFile(
                            $"moderation/moderated/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement",
                            $"moderation/denied/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");

                    }
                }
            }
            ini();
        }
        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            switch (IsAdmin)
            {
                case false:
                    NoAccess noAccess = new NoAccess("editing", (StatementsListBox.SelectedItem as StatementLoc) );
                    noAccess.ShowDialog();
                    break;
                case true:
                    if ((sender as DataGrid).ItemsSource == StatementListRN)
                    {
                        PopUps.Edit edit = new Edit((StatementsListBox.SelectedItem as StatementLoc), "n");
                        edit.Show();

                    }
                    else if ((sender as DataGrid).ItemsSource == UStatementListRN)
                    {
                        PopUps.Edit edit = new Edit((StatementsListBox.SelectedItem as StatementLoc), "u");
                        edit.Show();

                    }
                    else if ((sender as DataGrid).ItemsSource == MStatementListRN)
                    {
                        PopUps.Edit edit = new Edit((StatementsListBox.SelectedItem as StatementLoc), "m");
                        edit.Show();

                    }
                    break;
            }
        }
        private void Del_OnClick(object sender, RoutedEventArgs e)
        {
            switch (IsAdmin)
            {
                case false:
                    NoAccess noAccess = new NoAccess("deleting", (StatementsListBox.SelectedItem as StatementLoc) );
                    noAccess.ShowDialog();
                    break;
                case true:
                    var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
                    using (var con = new FtpClient("31.31.196.95", cr.Login, cr.Password))
                    {
                        con.Connect();
                        if (con.DirectoryExists("unmoderated"))
                        {
                            NoAccess noAccess2 = new NoAccess("deleting", (StatementsListBox.SelectedItem as StatementLoc) );
                            noAccess2.ShowDialog();
                        }
                        else if (con.DirectoryExists("main"))
                        {
                            if (MessageBox.Show("Are you sure, Archivist‽‽‽ thats kinda dangerous af!", "Deleting", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                con.DownloadFile("main.archive", "main/main.archive");
                                ZipFile.ExtractToDirectory("main.archive", "main.archive.edit", true);
                                File.Delete("main.archive");
                                File.Delete($"main.archive.edit/{(StatementsListBox.SelectedItem as StatementLoc).Title}.statement");
                                ZipFile.CreateFromDirectory("main.archive.edit", "main.archive");
                                con.UploadFile("main.archive", "main/main.archive", FtpRemoteExists.Overwrite);
                                Directory.Delete("main.archive.edit", true);   
                            }
                        }
                    }
                    break;
            }
        }
        //help
        private void Howtouse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                howtouse hOwtouse = new howtouse();
                hOwtouse.Show();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void Guidelines_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Guidelines guidelines = new Guidelines();
                guidelines.Show();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                About about = new About();
                about.Show();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }

        
        //sorting
        private void Upd0(object? sender, EventArgs e)
        {
            ini();
        }
        private void TreeViewItemOnSelected(object sender, RoutedEventArgs e)
        {
            try
            {
            StatementListRN.Clear();
            foreach (var sl in StatementsInf)
            {
                if (sl.Tags.Replace(" ", "_").Contains((sender as TreeViewItem).Name))
                {
                    StatementListRN.Add(sl);
                }
            }
            StatementsListBox.Items.Refresh();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void StatementsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
            if ((sender as DataGrid).SelectedItem != null)
            {
                statementmenu.IsEnabled = true;
                if ((sender as DataGrid).ItemsSource == StatementListRN)
                {
                    statement.Document = StatementsCont[Convert.ToInt32(((sender as DataGrid).SelectedItem as StatementLoc).LocId)];   
                }
                else if ((sender as DataGrid).ItemsSource == UStatementListRN)
                {
                    statement.Document = UStatementsCont[Convert.ToInt32(((sender as DataGrid).SelectedItem as StatementLoc).LocId)];
                }
                else if ((sender as DataGrid).ItemsSource == MStatementListRN)
                {
                    statement.Document = MStatementsCont[Convert.ToInt32(((sender as DataGrid).SelectedItem as StatementLoc).LocId)];
                }
            }
            else
            {
                statementmenu.IsEnabled = false;
            }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void TreeViewItem_OnSelected(object sender, MouseButtonEventArgs e)
        {
            try
            {
                StatementListRN.Clear();
            StatementListRN.AddRange(StatementsInf);
            StatementsListBox.Items.Refresh();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void Favs_OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            try
            {
            StatementListRN.Clear();
            foreach (var sl in StatementsInf)
            {
                if (mainConfig.Favs == null)
                {
                    break;
                }
                foreach (var fvid in mainConfig.Favs.Split("$"))
                {
                    if (sl.LocId.ToString() == fvid)
                    {
                        StatementListRN.Add(sl);
                    }                    
                }
            }
            StatementsListBox.Items.Refresh();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
        private void Unmoderated_OnSelected(object sender, RoutedEventArgs e)
        {
            StatementsListBox.ItemsSource = null;
            StatementsListBox.ItemsSource = UStatementListRN;
            Del.IsEnabled = false;
            switch (IsAdmin)
            {
                case false:
                    Accept.IsEnabled = false;
                    Deny.IsEnabled = false;
                    break;
                case true:
                    Accept.IsEnabled = true;
                    Deny.IsEnabled = true;
                    break;
            }
        }
        private void Moderated_OnSelected(object sender, RoutedEventArgs e)
        {
            Del.IsEnabled = false;
            StatementsListBox.ItemsSource = null;
            StatementsListBox.ItemsSource = MStatementListRN;
            switch (IsAdmin)
            {
                case false:
                    Accept.IsEnabled = false;
                    Deny.IsEnabled = false;
                    break;
                case true:
                    Accept.IsEnabled = true;
                    Deny.IsEnabled = true;
                    break;
            }
        }
        private void selected2(object sender, RoutedEventArgs e)
        {
            StatementsListBox.ItemsSource = null; 
            StatementsListBox.ItemsSource = StatementListRN;
            Accept.IsEnabled = false;
            Deny.IsEnabled = false;
            Del.IsEnabled = true;
        }

        private async void Chk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://triangleonthewall.org/fup.ini", "fup.ini");
                var up = new ConfigurationBuilder<updch>().UseIniFile("fup.ini").Build();
                if (up.latestver > 2)
                {
                    File.Delete("fup.ini");
                    if (MessageBox.Show("Update?", "Updates found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        client.DownloadFile("https://triangleonthewall.org/fsetup.exe", "fsetup.exe");
                        Process.Start("fsetup.exe");
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    File.Delete("fup.ini");
                    MessageBox.Show("No updates");
                }
            }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message);
            }
        }
    }
}

public interface updch
{
    public int latestver { get; set; }
}