using System.Windows;
using System.Windows.Input;

namespace Fr3_d1.PopUps;

public partial class About : Window
{
    public About()
    {
        InitializeComponent();
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
}