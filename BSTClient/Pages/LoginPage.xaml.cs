using BSTClient.API;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

//using MessageBox = HandyControl.Controls.MessageBox;

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
            var message = await Requester.Default.AuthenticationAsync(user, password);
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
                //MessageBox.Show("200", App.Current.MainWindow.Title, MessageBoxButton.OK,
                //    MessageBoxImage.Information);
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.DrawerLeft.Visibility = Visibility.Visible;

                mainWindow.MainFrame.Navigated += OnMainFrameOnNavigated;
                mainWindow.MainFrame?.Navigate(new DashboardPage());

                void OnMainFrameOnNavigated(object obj, NavigationEventArgs arg)
                {
                    ClearHistory(mainWindow.MainFrame);
                    mainWindow.MainFrame.Navigated -= OnMainFrameOnNavigated;
                }
            }

        }
        public static void ClearHistory(Frame frame)
        {
            if (frame == null) return;
            if (!frame.CanGoBack && !frame.CanGoForward)
            {
                return;
            }

            var entry = frame.RemoveBackEntry();
            while (entry != null)
            {
                entry = frame.RemoveBackEntry();
            }

            //frame.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
        }
    }
}

