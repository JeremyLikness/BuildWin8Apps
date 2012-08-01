using System.Windows;
using PortableWintellog;
using PortableWintellog.Contracts;
using PortableWintellog.Data;

namespace WintellogWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc = new TinyIoc();
            Ioc.Register<IStorageUtility>(ioc => new StorageUtility());
            Ioc.Register<IApplicationContext>(ioc => new ApplicationContext());
            Ioc.Register<IDialog>(ioc => new WpfDialog());
            Ioc.Register<ISyndicationHelper>(ioc => new SyndicationHelper());
            Ioc.Register(ioc => new BlogDataSource(ioc.Resolve<IStorageUtility>(),
                ioc.Resolve<IApplicationContext>(),
                ioc.Resolve<IDialog>(),
                ioc.Resolve<ISyndicationHelper>()));            
        }

        public ITinyIoc Ioc { get; private set; }

        public static App Instance
        {
            get { return ((App)Current); }
        }
    }
}
