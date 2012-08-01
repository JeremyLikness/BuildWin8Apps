using PortableWintellog.Contracts;

namespace WintellogTest.TestHelpers
{
    public class ApplicationContextTest : IApplicationContext 
    {
        public bool IsInDesigner { get;  set; }
        public bool IsTestMode { get; set; }
    }
}
