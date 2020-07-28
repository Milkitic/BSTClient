using BSTClient.API.Models.Response;
using System.Windows.Controls;

namespace BSTClient.Pages
{
    /// <summary>
    /// NotFoundPage.xaml 的交互逻辑
    /// </summary>
    public partial class NotFoundPage : Page
    {
        public NotFoundPage(ItemObj itemObj)
        {
            InitializeComponent();

            Title = "找不到模块 - " + itemObj.Name;
            DataContext = itemObj;
        }
    }
}
