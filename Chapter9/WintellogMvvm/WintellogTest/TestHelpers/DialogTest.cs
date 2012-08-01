using System.Threading.Tasks;
using PortableWintellog.Contracts;

namespace WintellogTest.TestHelpers
{
    public class DialogTest : IDialog 
    {
        public string Message { get; set; }

        public Task ShowDialogAsync(string dialog)
        {
            return Task.Run(() =>
                {
                    Message = dialog;
                });
        }
    }
}
