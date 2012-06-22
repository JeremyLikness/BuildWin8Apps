using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace EncryptionSigning
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private const string PASSWORD1 = "Password123";
        private const string PASSWORD2 = "Windows8IsFun!"; 

        private string _text;
        private byte[] _signature;
        private byte[] _encrypted;

        public MainPage()
        {
            InitializeComponent();
            LoadText();
        }

        private static IBuffer AsBuffer(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("str");
            }

            var buffer = CryptographicBuffer
                .ConvertStringToBinary(str.Trim(),
                BinaryStringEncoding.Utf8);

            return buffer;
        }

        private static string AsText(IBuffer bytes)
        {
            if (bytes == null || bytes.Length < 1)
            {
                throw new ArgumentNullException("bytes");
            }

            return CryptographicBuffer
                .ConvertBinaryToString(BinaryStringEncoding.Utf8,
                bytes);
        }

        private async void LoadText()
        {
            _text = await PathIO.ReadTextAsync(@"ms-appx:///Text.txt");
            BigTextBox.Text = _text;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void EncryptButton_Click_1(object sender, RoutedEventArgs e)
        {
            var message = string.Empty;
            try
            {
                var result = await GetPassword();
                var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("RC4");
                var key = provider.CreateSymmetricKey(AsBuffer(result));
                var encrypted = CryptographicEngine.Encrypt(key,
                    AsBuffer(BigTextBox.Text), null);
                _encrypted = encrypted.ToArray();
                BigTextBlock.Text = CryptographicBuffer.EncodeToBase64String(encrypted);
                DecryptButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            if (!string.IsNullOrEmpty(message))
            {
                await SimpleDialog(message);
            }
        }

        private async void DecryptButton_Click_1(object sender, RoutedEventArgs e)
        {
            var message = string.Empty;
            try
            {
                var result = await GetPassword();
                var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("RC4");
                var key = provider.CreateSymmetricKey(AsBuffer(result));
                var decrypted = CryptographicEngine.Decrypt(key,
                    _encrypted.AsBuffer(), null);
                BigTextBox.Text = AsText(decrypted).Trim();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            if (!string.IsNullOrEmpty(message))
            {
                await SimpleDialog(message);
            }
        }

        private async void SignButton_Click_1(object sender, RoutedEventArgs e)
        {
            var provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA256");
            var key = provider.CreateKey(
                AsBuffer(MakeBigPassword(PASSWORD1)));
            _signature = CryptographicEngine.Sign(key, 
                AsBuffer(BigTextBox.Text)).ToArray();                       
            await SimpleDialog("The content has been signed.");
            VerifyButton.IsEnabled = true;
        }

        private async void VerifyButton_Click_1(object sender, RoutedEventArgs e)
        {
            var provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA256");
            var key = provider.CreateKey(
                AsBuffer(MakeBigPassword(PASSWORD1)));
            var result = CryptographicEngine.VerifySignature(key,
                AsBuffer(BigTextBox.Text),
                _signature.AsBuffer());
            await SimpleDialog(result ? "The signature was valid. Your data was not modified." :
                "The signature is invalid. The data has been tampered with since it was last signed.");
        }

        private async Task SimpleDialog(string str)
        {
            var messageDialog = new MessageDialog(str);
            messageDialog.Commands.Add(new UICommand("OK"));
            await messageDialog.ShowAsync();
        }

        private async Task<string> GetPassword()
        {
            var result = PASSWORD1;
            var messageDialog = new MessageDialog("Please select a password to use.");

            messageDialog.Commands.Add(
                new UICommand(PASSWORD1, args => result = MakeBigPassword(PASSWORD1)));
            messageDialog.Commands.Add(
                new UICommand(PASSWORD2,
                    args => result = MakeBigPassword(PASSWORD2)));
            
            // Set the command to be invoked when a user presses 'ENTER' 
            messageDialog.DefaultCommandIndex = 1;

            // Show the message dialog 
            await messageDialog.ShowAsync();

            return result; 
        }

        private static string MakeBigPassword(string src)
        {
            var sb = new StringBuilder(src);
            for (var x = 0; x < 100; x++)
            {
                sb.Append(src);
            }
            return sb.ToString();
        }
    }    
}
