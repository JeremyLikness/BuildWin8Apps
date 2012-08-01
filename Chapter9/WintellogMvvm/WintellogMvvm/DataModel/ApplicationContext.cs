using PortableWintellog.Contracts;
using Windows.ApplicationModel;

namespace WintellogMvvm.DataModel
{
    public class ApplicationContext : IApplicationContext 
    {
        public bool IsInDesigner
        {
            get { return DesignMode.DesignModeEnabled; }
        }
        
        public bool IsTestMode
        {
            get { return Config.TestOnly; }
        }
    }
}
