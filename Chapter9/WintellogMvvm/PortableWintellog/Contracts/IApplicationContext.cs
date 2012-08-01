namespace PortableWintellog.Contracts
{
    public interface IApplicationContext
    {
        bool IsInDesigner { get; }
        bool IsTestMode { get; }
    }
}