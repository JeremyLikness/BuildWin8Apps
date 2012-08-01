using System;
using System.Threading.Tasks;
using PortableWintellog.Contracts;
using Windows.UI.Popups;

namespace WintellogMvvm.DataModel
{
    public class Dialog : IDialog 
    {
        public async Task ShowDialogAsync(string dialog)
        {
            var dlg = new MessageDialog(dialog);
            await dlg.ShowAsync();
        }
    }
}
