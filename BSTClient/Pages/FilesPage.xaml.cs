using BSTClient.API;
using BSTClient.API.Models.Response;
using BSTClient.Helpers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BSTClient.Pages
{
    public class FilesPageVm : VmBase
    {
        private MyDirectoryObject _directoryObject;
        private long _current;
        private long _total;

        public MyDirectoryObject DirectoryObject
        {
            get => _directoryObject;
            set
            {
                if (Equals(value, _directoryObject)) return;
                _directoryObject = value;
                OnPropertyChanged();
            }
        }

        public long Current
        {
            get => _current;
            set
            {
                if (value == _current) return;
                _current = value;
                OnPropertyChanged();
            }
        }

        public long Total
        {
            get => _total;
            set
            {
                if (value == _total) return;
                _total = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand => new DelegateCommand(async arg =>
        {
            var fixedPath =
                FilesPage.FixRelativePath(DirectoryObject.RelativePath, Path.DirectorySeparatorChar);
            var (success, message, directoryObject) =
                await Requester.Default.GetDirectoryInfo(fixedPath);
            if (success)
            {
                DirectoryObject = MyDirectoryObject.FromDirectoryObject(directoryObject);
            }
            else
            {
                MessageBox.Show(message);
            }
        });

        public ICommand ParentCommand => new DelegateCommand(async arg =>
        {
            var fixedPath =
                FilesPage.FixRelativePath(DirectoryObject.RelativePath, Path.DirectorySeparatorChar);
            var split = fixedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0) return;

            var newPath = string.Join(Path.DirectorySeparatorChar, split.Take(split.Length - 1));

            var (success, message, directoryObject) =
                await Requester.Default.GetDirectoryInfo(newPath);
            if (success)
            {
                DirectoryObject = MyDirectoryObject.FromDirectoryObject(directoryObject);
            }
            else
            {
                MessageBox.Show(message);
            }
        });

        public ICommand UploadCommand => new DelegateCommand(arg =>
        {
            var ofd = new OpenFileDialog();
            var b = ofd.ShowDialog();
            if (b != true) return;
            try
            {
                UploadManager.Default.AddTask(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        });
    }

    /// <summary>
    /// FilesPage.xaml 的交互逻辑
    /// </summary>
    public partial class FilesPage : Page
    {
        private bool _loaded;
        private FilesPageVm _viewModel;

        public FilesPage()
        {
            InitializeComponent();
            _viewModel = (FilesPageVm)DataContext;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                var (success, message, directoryObject) = await Requester.Default.GetDirectoryInfo("");
                if (success)
                {
                    _viewModel.DirectoryObject = MyDirectoryObject.FromDirectoryObject(directoryObject);
                }
                else
                {
                    MessageBox.Show(message);
                }

                _loaded = true;
            }
        }

        private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dg = (DataGrid)sender;
            if (!(e.OriginalSource is FrameworkElement fe)) return;

            var obj = fe.FindParentObjects(typeof(DataGridRow));
            if (obj == null) return;

            var item = (ExplorerDesc)dg.SelectedItem;
            if (item is FileDesc fd)
            {

            }
            else if (item is DirectoryDesc dd)
            {
                var relativePath = FixRelativePath(dd.RelativePath, _viewModel.DirectoryObject.DirectorySeparatorChar);
                var (success, message, directoryObject) = await Requester.Default.GetDirectoryInfo(relativePath);
                if (success)
                {
                    _viewModel.DirectoryObject = MyDirectoryObject.FromDirectoryObject(directoryObject);
                }
                else
                {
                    MessageBox.Show(message);
                }
            }
        }

        public static string FixRelativePath(string rel, char c)
        {
            if (rel == "~")
                return string.Empty;
            var root = "~" + c;
            var relativePath = rel?.TrimStart(root);
            return relativePath;
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                var textBox = (TextBox)sender;
                var path = textBox.Text;
                var fixedPath = FixRelativePath(path, Path.DirectorySeparatorChar);
                var (success, message, directoryObject) = await Requester.Default.GetDirectoryInfo(fixedPath);
                if (success)
                {
                    _viewModel.DirectoryObject = MyDirectoryObject.FromDirectoryObject(directoryObject);
                    //BtnRefresh.Focus();
                    TbPath.SelectionStart = TbPath.Text.Length;
                    TbPath.SelectionLength = 0;
                }
                else
                {
                    TbPath.SelectAll();
                    MessageBox.Show(message);
                }
            }
        }

        private void TbPath_LostFocus(object sender, RoutedEventArgs e)
        {
            //_viewModel.DirectoryObject.RelativePath = _viewModel.DirectoryObject.RelativePath;
            //TbPath.SelectionLength = 0;
        }
    }
}
