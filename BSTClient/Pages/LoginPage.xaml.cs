using BSTClient.API;
using System.Windows;
using System.Windows.Controls;
//using MessageBox = HandyControl.Controls.MessageBox;

namespace BSTClient.Pages
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        readonly Requester _req = new Requester();
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var user = this.tbUser.Text;
            var password = tbPass.Password;
            var message = await _req.AuthenticationAsync(user, password);
            if (message != null)
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
                MessageBox.Show("200", App.Current.MainWindow.Title, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

        }
    }
}

