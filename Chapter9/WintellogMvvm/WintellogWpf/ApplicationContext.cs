using System.ComponentModel;
using System.Windows;
using PortableWintellog.Contracts;

namespace WintellogWpf
{
    public class ApplicationContext : IApplicationContext 
    {
        public bool IsInDesigner
        {
            get { return DesignerProperties.GetIsInDesignMode(new DependencyObject()); }
        }
        public bool IsTestMode
        {
            get { return false; }
        }        
    }
}
