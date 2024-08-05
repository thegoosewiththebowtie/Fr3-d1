using System.Windows;
using System.Windows.Input;
using Config.Net;
using FluentFTP;

namespace Fr3_d1.PopUps;

public interface Credentials
{
    public string Login { get; set; }
    public string Password { get; set; }
}
public partial class LogIn : Window
{
    public LogIn()
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

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            using( var con = new FtpClient("31.31.196.95", login.Text, password.Text))
            {
                con.Connect();
                con.Disconnect();
            }
            var cr = new ConfigurationBuilder<Credentials>().UseIniFile("credentials").Build();
            cr.Login = login.Text;
            cr.Password = password.Text;
            MessageBox.Show("Success!");
            this.Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        
    }
}