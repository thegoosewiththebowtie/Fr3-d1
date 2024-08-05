using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Config.Net;

namespace Fr3_d1.PopUps;

public interface Statement
{
    public int LocId { get; set; }
    public string Title { get; set; }
    public string Regarding { get; set; }
    public string StatementOf { get; set; }
    public string StatementGiven { get; set; }
    public string StatementDigitized { get; set; }
    public string DigitizedBy { get; set; }
    public string Tags { get; set; }
    public string TheStatement { get; set; }
}


public class StatementLoc
{
    public int LocId { get; set; }
    public string Title { get; set; }
    public string Regarding { get; set; }
    public string StatementOf { get; set; }
    public string StatementGiven { get; set; }
    public string StatementDigitized { get; set; }
    public string DigitizedBy { get; set; }
    public string Tags { get; set; }
    public string TheStatement { get; set; }
}


public partial class NewStatement : Window
{
    public ArchiveConfig archiveConfig { get; set; }
    public NewStatement()
    {
        InitializeComponent();
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
        foreach (var v in archiveConfig.tags.Split("$"))
        {
            System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox();
            checkBox.Content = v;
            tags.Children.Add(checkBox);
        }

        if (Directory.Exists("dirdraft.statement"))
        {
            Directory.Delete("dirdraft.statement", true);
        }
        if (File.Exists("draft.statement"))
        {
            ZipFile.ExtractToDirectory("draft.statement", "dirdraft.statement");
            Statement draft = new ConfigurationBuilder<Statement>().UseIniFile("dirdraft.statement/draft.statementinfo").Build();
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
            fStream = new FileStream("dirdraft.statement/draft.statementcontent", FileMode.OpenOrCreate);
            range.Load(fStream, System.Windows.DataFormats.XamlPackage);
            fStream.Close();
            Directory.Delete("dirdraft.statement", true);
        }
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

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        if (!File.Exists($"{ConstantVars.StatementsPath}/config.ini"))
        {
            MessageBox.Show("Please, sync with The Web.");
            return;
        }
        try
        {
        Directory.CreateDirectory("dirdraft.statement");
        Statement draft = new ConfigurationBuilder<Statement>().UseIniFile("dirdraft.statement/draft.statementinfo").Build();
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
        fStream = new FileStream("dirdraft.statement/draft.statementcontent", FileMode.Create);
        range.Save(fStream, System.Windows.DataFormats.XamlPackage);
        fStream.Close();
        fStream.Dispose();
        if (File.Exists("draft.statement"))
        {
            File.Delete("draft.statement");
        }
        draft = null;
        ZipFile.CreateFromDirectory("dirdraft.statement", "draft.statement");
        Directory.Delete("dirdraft.statement", true);
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
        if (File.Exists($"{nm}.statement"))
        {
            File.Delete($"{nm}.statement");
        }
        draft = null;
        ZipFile.CreateFromDirectory($"dir{nm}.statement", $"{nm}.statement");
        Directory.Delete($"dir{nm}.statement", true);
        try
        {
            System.Net.WebClient Client = new System.Net.WebClient();
            byte[] result = Client.UploadFile(ConstantVars.UploadLink, "POST",
                $"{nm}.statement");
            string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length); 
            MessageBox.Show(s);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        File.Delete($"{nm}.statement");
        File.Delete("draft.statement");
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(exception.Message);
        }
    }
}