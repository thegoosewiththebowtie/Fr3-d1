using System.Windows;
using System.Windows.Input;

namespace Fr3_d1.PopUps;

public partial class NoAccess : Window
{
    public NoAccess(string noaccess)
    {
        InitializeComponent();
        
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
    
}