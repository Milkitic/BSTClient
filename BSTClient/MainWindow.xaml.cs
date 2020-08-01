using BSTClient.API;
using BSTClient.API.Models.Response;
using BSTClient.Pages;
using HandyControl.Controls;
using HandyControl.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace BSTClient
{
    public class MainWindowVm : VmBase
    {
        private static Page _dashboardPage = new DashboardPage();
        private static Page _filesPage = new FilesPage();

        public ICommand SelectCmd => new DelegateCommand<ItemObj>(item =>
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var frame = mainWindow.MainFrame;

            switch (item.Tag)
            {
                case "dashboard":
                    frame.Navigate(_dashboardPage);
                    break;
                case "files":
                    frame.Navigate(_filesPage);
                    break;
                default:
                    frame.Navigate(new NotFoundPage(item));
                    break;
            }
        });
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Requester _requester;
        private MainWindowVm _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DrawerLeft.Visibility = Visibility.Collapsed;
            _viewModel = (MainWindowVm)DataContext;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _requester = new Requester();
            var result = await _requester.TestAuthenticationAsync();
            if (result != null)
            {
                var loginPage = new LoginPage();
                MainFrame.Navigate(loginPage);
            }
            else
            {
                var (success, message, navObj) = await _requester.GetNavigatorInfo();
                if (success)
                {
                    var style = (Style)Application.Current.FindResource("TextBlockFabricIcons");
                    var brush = (Brush)Application.Current.FindResource("SecondaryRegionBrush");
                    //var brush = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                    //var foreBrush = new SolidColorBrush(Color.FromRgb(48, 68, 128));
                    var foreBrush = (Brush)Application.Current.FindResource("SecondaryTextBrush");
                    DrawerLeft.Visibility = Visibility.Visible;
                    //_viewModel.NavSections = navObj.Sections;
                    foreach (var g in navObj.Sections)
                    {
                        var sideMenuItem = new SideMenuItem
                        {
                            Background = brush,
                            Header = new TextBlock
                            {
                                Text = g.Name,
                            },
                            Icon = new TextBlock
                            {
                                Style = style,
                                Text = g.IconString
                            },
                            FontSize = 15,
                            Tag = g.Tag
                        };

                        foreach (var itemObj in g.Items)
                        {
                            var menuItem = new SideMenuItem
                            {
                                Header = itemObj.Name,
                                Icon = new TextBlock
                                {
                                    Style = style,
                                    Text = itemObj.IconString
                                },
                                Tag = itemObj.Tag,
                                FontSize = 12,
                                Foreground = foreBrush,
                                Command = _viewModel.SelectCmd,
                                CommandParameter = itemObj
                            };
                            sideMenuItem.Items.Add(menuItem);
                        }
                        GeneralSideMenu.Items.Add(sideMenuItem);
                    }

                    GeneralSideMenu.ExpandMode = ExpandMode.Freedom;
                    //MainFrame.NavigationUIVisibility = NavigationUIVisibility.Visible;
                    //LblUri.Visibility = Visibility.Visible;
                    //if (firstItem != null)
                    //{
                    //    firstItem.IsSelected = true;
                    //}
                }
                else
                {
                    MessageBox.Show(message, App.Current.MainWindow.Title, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
                //MainFrame.Navigate(new GeneralPage());
            }
        }
    }
}
