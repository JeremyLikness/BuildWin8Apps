using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace PushNotificationTest
{
    public class Authentication
    {
        [DataContract]
        public class OAuthToken
        {
            [DataMember(Name = "access_token")]
            public string AccessToken { get; set; }

            [DataMember(Name = "token_type")]
            public string TokenType { get; set; }
        }

        private static OAuthToken GetOAuthTokenFromJson(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                var ser = new DataContractJsonSerializer(typeof (OAuthToken));
                var oAuthToken = (OAuthToken) ser.ReadObject(ms);
                return oAuthToken;
            }
        }

        public async Task<string> GetAccessToken()
        {
            const string SECRET = "4n3ZeAodQpNmAQiBIpl2Pb6gcyKetITN";
            const string SID =
                "ms-app://s-1-15-2-995128278-2162023023-1185128777-573137552-950875679-3130306181-1280117587";
            var urlEncodedSid = WebUtility.UrlEncode(String.Format("{0}", SID));
            var urlEncodedSecret = WebUtility.UrlEncode(SECRET);

            var body =
                String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com",
                              urlEncodedSid, urlEncodedSecret);

            var client = new HttpClient();

            var httpBody = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response =
                await client.PostAsync(new Uri("https://login.live.com/accesstoken.srf", UriKind.Absolute), httpBody);
            var oAuthToken = GetOAuthTokenFromJson(await response.Content.ReadAsStringAsync());
            return oAuthToken.AccessToken;
        }
    }
}