﻿using System.Windows;

namespace BSTClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UploadManager _uploadManage;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _uploadManage = new UploadManager();
        }
    }
}
