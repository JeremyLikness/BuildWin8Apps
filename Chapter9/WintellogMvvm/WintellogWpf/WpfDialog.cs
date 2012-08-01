using System.Threading.Tasks;
using System.Windows;
using PortableWintellog.Contracts;

namespace WintellogWpf
{
    public class WpfDialog : IDialog 
    {
        public async Task ShowDialogAsync(string dialog)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => MessageBox.Show(dialog));            
        }
    }
}
