using System.Threading.Tasks;

namespace PortableWintellog.Contracts
{
    public interface IDialog
    {
        Task ShowDialogAsync(string dialog);
    }
}