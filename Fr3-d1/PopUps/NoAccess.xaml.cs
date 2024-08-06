using System.Media;
using System.Windows;
using System.Windows.Input;

namespace Fr3_d1.PopUps;

public partial class NoAccess : Window
{
    public string noaccess2 { get; set; }
    private StatementLoc statementInfo { get; set; } = new StatementLoc();
    public NoAccess(string noaccess, StatementLoc statementinfo)
    {
        InitializeComponent();
        SoundPlayer soundPlayer = new SoundPlayer();
        soundPlayer.SoundLocation = "Sounds/CHORD.wav";
        soundPlayer.Load();
        soundPlayer.Play();
        info.Text = info.Text.Replace("[thisaction]", noaccess);
        statementInfo = statementinfo;
        noaccess2 = noaccess;
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

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void SuggestionButton_OnClick(object sender, RoutedEventArgs e)
    {
        switch (noaccess2)
        {
            case "editing":
                Edit edit = new Edit(statementInfo, "n");
                edit.Show();
                this.Close();
                break;
            case "deleting":
                Report report = new Report(statementInfo);
                report.Show();
                this.Close();
                break;
        }
    }
}