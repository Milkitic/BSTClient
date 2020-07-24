using BSTClient.Credentials;
using System.Windows;

namespace BSTClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var list = CredentialManager.EnumerateCrendentials();
            if (UserPasswordManager.TryGet(out string uname, out string pword))
            {

            }
            UserPasswordManager.Set("asdf", "1234");
        }
    }
}
