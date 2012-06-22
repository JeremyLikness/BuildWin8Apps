using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Compression;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Compression
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Byte[] _compressedText;
        private string _text = string.Empty;

        public MainPage()
        {
            InitializeComponent();
            LoadText();
            DecompressButton.IsEnabled = false;
        }

        private async void LoadText()
        {
            _text = await PathIO.ReadTextAsync(@"ms-appx:///Text.txt");
            await SimpleDialog(string.Format("Loaded text with {0} bytes", _text.Length));
            BigText.Text = _text;
        }

        private static async Task SimpleDialog(string text)
        {
            var msg = new MessageDialog(text);
            await msg.ShowAsync();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var storage = await ApplicationData.Current.LocalFolder
                                    .CreateFileAsync("compressed.zip",
                                                     CreationCollisionOption.
                                                         ReplaceExisting);

            var bytes = Encoding.UTF8.GetBytes(_text);

            using (var stream = await storage.OpenStreamForWriteAsync())
            {
                var compressor = new Compressor(stream.AsOutputStream());
                await compressor.WriteAsync(bytes.AsBuffer());
                await compressor.FinishAsync();
            }

            var compressedBuffer = await FileIO.ReadBufferAsync(storage);

            _compressedText = compressedBuffer.ToArray();

            await SimpleDialog(string.Format("Compressed {0} bytes to {1}",
                                             _text.Length,
                                             compressedBuffer.Length));

            DecompressButton.IsEnabled = true;
            CompressButton.IsEnabled = false;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var decompressed = await ApplicationData.Current.LocalFolder
                                         .CreateFileAsync("decompressed.txt",
                                                          CreationCollisionOption
                                                              .ReplaceExisting);

            var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("compressed.zip");

            var decompressor = new Decompressor(stream.AsInputStream());

            var bytes = new Byte[100000];
            var buffer = bytes.AsBuffer();

            var buf = await decompressor.ReadAsync(buffer, 999999, InputStreamOptions.None);

            await FileIO.WriteBufferAsync(decompressed, buf);

            await SimpleDialog(string.Format("Decompressed {0} bytes to {1}",
                                             _compressedText.Length,
                                             buf.Length));

            BigText.Text = new String(Encoding.UTF8.GetChars(buf.ToArray()));

            DecompressButton.IsEnabled = false;
        }
    }
}