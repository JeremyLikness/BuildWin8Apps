using System.Windows;
using System.Windows.Controls;
using PortableWintellog.Data;

namespace WintellogWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Splash.Visibility = Visibility.Visible;
            Groups.Visibility = Visibility.Collapsed;
            Loaded += MainWindow_Loaded;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var dataSource = App.Instance.Ioc.Resolve<BlogDataSource>();
            await dataSource.LoadGroups();

            foreach (var group in dataSource.GroupList)
            {
                Status.Text = string.Format("Loading {0}...", group.Title);
                await dataSource.LoadAllItems(group);
            }
            
            Splash.Visibility = Visibility.Collapsed;
            Groups.DataContext = dataSource;
            Groups.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            
            if (button == null)
            {
                return;
            }

            var dataContext = button.DataContext as BlogItem;

            if (dataContext != null)
            {
                System.Diagnostics.Process.Start(dataContext.PageUri.ToString());
            }
        }
    }
}
