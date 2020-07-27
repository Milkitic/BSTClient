using System.Windows;
using BSTClient.API;
using System.Windows.Controls;
using HandyControl.Data;
using MessageBox = HandyControl.Controls.MessageBox;

namespace BSTClient.Pages
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var user = this.tbUser.Text;
            var password = tbPass.Password;
            var req = new Requester();
            var (success, message) = await req.AuthenticationAsync(user, password);
            if (!success)
            {
                //new MessageBoxInfo()
                //{
                //    Button = MessageBoxButton.OK,
                //    Caption = App.Current.MainWindow.Title,
                //    Message = message,
                //}
                MessageBox.Show(message, App.Current.MainWindow.Title, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show(message, App.Current.MainWindow.Title, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }

        }
    }
}

