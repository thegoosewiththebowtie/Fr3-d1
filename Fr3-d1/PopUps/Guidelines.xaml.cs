using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Fr3_d1.PopUps;

public partial class Guidelines : Window
{
    public Guidelines()
    {
        InitializeComponent();
        ini();
    }
    private void collapse_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = System.Windows.WindowState.Minimized;
    }
    

    private void close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void ini()
    {
        if (File.Exists($"{ConstantVars.StatementsPath}/guidelines.rtf"))
        {
            TextRange range;
            FileStream fStream;
            range = new TextRange(guidelines.Document.ContentStart, guidelines.Document.ContentEnd);
            fStream = new FileStream($"{ConstantVars.StatementsPath}/guidelines.rtf", FileMode.OpenOrCreate);
            range.Load(fStream, System.Windows.DataFormats.Rtf);
            fStream.Close();
        }
        else
        {
            MessageBox.Show("You have to sync with The Web to get up-to-date guidelines");
        }
    }
}